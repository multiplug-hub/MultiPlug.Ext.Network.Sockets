using System;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using MultiPlug.Base.Exchange;
using MultiPlug.Base.Exchange.API;
using MultiPlug.Ext.Network.Sockets.Models.Components;
using MultiPlug.Ext.Network.Sockets.Diagnostics;

namespace MultiPlug.Ext.Network.Sockets.Components.SocketClient
{
    public class SocketClientComponent : SocketClientProperties
    {
        public event Action EventsUpdated;
        public event Action SubscriptionsUpdated;

        private MessageBuffer m_MessageBuffer;

        private Socket m_Socket;

        private bool m_ConnectionInitialising = false;
        private bool m_ConnectionInError = false;

        private ILoggingService m_LoggingService;

        public string LogEventId { get { return m_LoggingService.EventId; } }

        private System.Timers.Timer m_InitialiseDelayTimer;

        public SocketClientComponent( string theGuid, ILoggingService theLoggingService)
        {
            Guid = theGuid;
            ReadEvent = new Event { Guid = theGuid, Id = System.Guid.NewGuid().ToString(), Description = "", Subjects = new[] { "value" } };
            ReadEvent.Enabled = false;
            WriteSubscriptions = new Subscription[0];
            m_LoggingService = theLoggingService;

            m_MessageBuffer = new MessageBuffer(theGuid, "SocketClient");

            SubscriptionsControlConnect = true;
        }
        internal void UpdateProperties(SocketClientProperties theNewProperties)
        {
            bool SubscriptionsUpdatedFlag = false;
            bool EventsUpdatedFlag = false;

            bool ForceInitialise = false;

            if (theNewProperties.Guid == null || theNewProperties.Guid != Guid)
            {
                return;
            }

            if (theNewProperties.SubscriptionsControlConnect != null && SubscriptionsControlConnect != theNewProperties.SubscriptionsControlConnect)
            {
                SubscriptionsControlConnect = theNewProperties.SubscriptionsControlConnect;

                if ( SubscriptionsControlConnect == false )
                {
                    ForceInitialise = true;
                }
            }

            if (theNewProperties.ReadEvent != null)
            {
                if (Event.Merge(ReadEvent, theNewProperties.ReadEvent))
                {
                    EventsUpdatedFlag = true;
                }
            }

            if (theNewProperties.HostName != null && theNewProperties.HostName != HostName)
            {
                HostName = theNewProperties.HostName;
                ForceInitialise = true;
            }
            if (theNewProperties.Port != -1 && theNewProperties.Port != Port)
            {
                Port = theNewProperties.Port;
                ForceInitialise = true;
            }

            if (theNewProperties.LoggingLevel != -1 && theNewProperties.LoggingLevel != LoggingLevel)
            {
                LoggingLevel = theNewProperties.LoggingLevel;
            }

            if (theNewProperties.WriteSubscriptions != null)
            {
                List<Subscription> NewSubscriptions = new List<Subscription>();

                foreach ( Subscription Subscription in theNewProperties.WriteSubscriptions)
                {
                    Subscription Search = WriteSubscriptions.FirstOrDefault(ne => ne.Guid == Subscription.Guid);

                    if( Search == null)
                    {
                        NewSubscriptions.Add(Subscription);
                    }
                    else
                    {
                        if (Subscription.Merge(Search, Subscription))
                        {
                            SubscriptionsUpdatedFlag = true;
                        }
                    }
                }

                if (NewSubscriptions.Count() > 0)
                {
                    SubscriptionsUpdatedFlag = true;
                }

                foreach (Subscription Subscription in NewSubscriptions)
                {
                    Subscription.Guid = System.Guid.NewGuid().ToString();
                    Subscription.Event += OnSubscriptionEvent;
                    Subscription.EnabledStatus += OnSubscriptionStatusChanged;
                }

                List<Subscription> List = WriteSubscriptions == null ? new List<Subscription>() : WriteSubscriptions.ToList();

                List.AddRange(NewSubscriptions);
                WriteSubscriptions = List.ToArray();
            }

            if (EventsUpdatedFlag)
            {
                EventsUpdated?.Invoke();
            }

            if (SubscriptionsUpdatedFlag)
            {
                SubscriptionsUpdated?.Invoke();
            }

            if (ForceInitialise)
            {
                OnLogWriteEntry(EventLogEntryCodes.SocketClientSocketClosingDueToReconfigure, new string[0]);
                Shutdown();

                if(SubscriptionsControlConnect == true)
                {
                    OnSubscriptionStatusChanged(false);
                }
                else
                {
                    InitialiseSetup();
                }
            }
        }

        internal string TraceLog
        {
            get
            {
                return (m_LoggingService == null) ? string.Empty : string.Join(System.Environment.NewLine, m_LoggingService.Read());
            }
        }

        private void OnSubscriptionStatusChanged(bool isEnabled /*Unused*/)
        {
            if( SubscriptionsControlConnect == true )
            {
                if( WriteSubscriptions.All(Subscription => Subscription.Enabled) )
                {
                    if( m_Socket == null || ( ! m_Socket.Connected ) )
                    {
                        InitialiseSetup();
                    }
                }
                else
                {
                    if (LoggingLevel > 0)
                    {
                        OnLogWriteEntry(EventLogEntryCodes.SocketClientSocketClosingDueToSubscriptionControl, new string[0]);
                    }

                    Shutdown();
                }
            }
        }

        private void OnSubscriptionEvent(SubscriptionEvent theSubscriptionEvent)
        {
            foreach(var Subject in theSubscriptionEvent.PayloadSubjects)
            {
                if (m_Socket != null && m_Socket.Connected)
                {
                    Send(Encoding.ASCII.GetBytes(Subject.Value));
                    if (LoggingLevel == 1)
                    {
                        OnLogWriteEntry(EventLogEntryCodes.SocketClientSending, new string[] { string.Empty });
                    }
                    else if (LoggingLevel == 2)
                    {
                        OnLogWriteEntry(EventLogEntryCodes.SocketClientSending, new string[] { theSubscriptionEvent.Payload.Subjects[0].Value });
                    }
                }
                else
                {
                    m_MessageBuffer.Enqueue(theSubscriptionEvent.Payload.Subjects[0].Value, theSubscriptionEvent.Payload.TimeToLive);
                }
            }
        }

        internal void RemoveSubcription(string theSubcriptionGuid)
        {
            Subscription Search = WriteSubscriptions.FirstOrDefault(s => s.Guid == theSubcriptionGuid);

            if (Search != null)
            {
                Search.Event -= OnSubscriptionEvent;
                Search.EnabledStatus -= OnSubscriptionStatusChanged;

                List<Subscription> SubscriptionsList = WriteSubscriptions.ToList();
                SubscriptionsList.Remove(Search);
                WriteSubscriptions = SubscriptionsList.ToArray();

                SubscriptionsUpdated?.Invoke();
            }
        }

        internal void Shutdown()
        {
            if( m_Socket != null )
            {
                try
                {
                    if (m_Socket.Connected)
                    {
                        m_Socket.Shutdown(SocketShutdown.Both);
                        m_Socket.Close();
                        m_Socket = null;

                        if (LoggingLevel > 0)
                        {
                            OnLogWriteEntry(EventLogEntryCodes.SocketClientSocketClosed, new string[0]);
                        }
                    }
                }
                catch(Exception theException)
                {
                    OnLogWriteEntry(EventLogEntryCodes.SocketClientSocketClosedException, new string[] { theException.Message });
                }

                ReadEvent.Enabled = false;
            }
        }

        private void InitialiseSetup()
        {
            if(m_ConnectionInitialising)
            {
                OnLogWriteEntry(EventLogEntryCodes.SocketClientAlreadyInitialising, null);
                return;
            }

            if( string.IsNullOrEmpty(HostName) )
            {
                OnLogWriteEntry(EventLogEntryCodes.SocketClientNoHostName, null);
                return;
            }

            if (Port == 0)
            {
                OnLogWriteEntry(EventLogEntryCodes.SocketClientIncorrectPort, null);
                return;
            }

            m_ConnectionInitialising = true;

            try
            {
                if( m_Socket != null )
                {
                    Shutdown();
                }

                IPAddress IPAddress;

                if ( ! System.Net.IPAddress.TryParse(HostName, out IPAddress) )
                {
                    IPHostEntry ipHostInfo = Dns.GetHostEntry(HostName);
                    IPAddress = ipHostInfo.AddressList[0];
                } 

                IPEndPoint remoteEP = new IPEndPoint(IPAddress, Port);

                m_Socket = new Socket(IPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                if (LoggingLevel > 0)
                {
                    OnLogWriteEntry(EventLogEntryCodes.SocketClientConnectingTo, new string[] { IPAddress.ToString() });
                }
                 
                m_Socket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), m_Socket);
            }
            catch (SocketException theSocketException)
            {
                OnSocketException(theSocketException);
            }
            catch (Exception theException)
            {
                OnLogWriteEntry(EventLogEntryCodes.SocketClientException, new string[] { theException.Message });
            }

            m_ConnectionInitialising = false;
        }

        private void InitialiseAfterDelay()
        {
            if(m_InitialiseDelayTimer != null && ! m_InitialiseDelayTimer.Enabled)
            {
                m_InitialiseDelayTimer = new System.Timers.Timer(1500);
                m_InitialiseDelayTimer.AutoReset = false;
                m_InitialiseDelayTimer.Elapsed += (s, e) => { InitialiseSetup(); };
                m_InitialiseDelayTimer.Start();
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            { 
                Socket client = (Socket)ar.AsyncState;

                client.EndConnect(ar);

                ConnectionInError = false;
                ReadEvent.Enabled = true;

                if ( LoggingLevel > 0)
                {
                    OnLogWriteEntry(EventLogEntryCodes.ConnectedTo, new string[] { client.RemoteEndPoint.ToString() });
                }

                Receive(client);

                string item = m_MessageBuffer.Peek();
                while ( item != string.Empty)
                {
                    byte[] byteData = Encoding.ASCII.GetBytes(item);

                    if( !Send(byteData) )
                    {
                        break;

                    }

                    m_MessageBuffer.Dequeue();
                    item = m_MessageBuffer.Peek();
                }
            }
            catch (ObjectDisposedException theException)
            {
                // Connection Closed
                ConnectionInError = true;
                OnLogWriteEntry(EventLogEntryCodes.SocketClientObjectDisposedException, new string[] { "ConnectCallback " + theException.Message });
                InitialiseAfterDelay();
            }
            catch (SocketException theException)
            {
                OnSocketException(theException);
            }
            catch (Exception theException)
            {
                OnLogWriteEntry(EventLogEntryCodes.SocketClientException, new string[] { theException.Message });
            }
        }

        private void Receive(Socket client)
        {
            try
            {
                SocketState state = new SocketState();
                state.workSocket = client;

                client.BeginReceive(state.buffer, 0, SocketState.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch( SocketException theException)
            {
                OnSocketException(theException);
            }
            catch (Exception theException)
            {
                OnLogWriteEntry(EventLogEntryCodes.SocketClientException, new string[] { theException.Message });
            }
        }

        private void ReceiveCallback(IAsyncResult theAsyncResult)
        {
            SocketState state = (SocketState)theAsyncResult.AsyncState;

            try
            {
                Socket client = state.workSocket; 

                if( ! client.Connected )
                {
                    ConnectionInError = true;
                    OnLogWriteEntry(EventLogEntryCodes.SocketClientClosedWhileReceive, new string[0]);
                    InitialiseAfterDelay();
                    return;
                }

                int bytesRead = client.EndReceive(theAsyncResult);

                ConnectionInError = false;

                if (bytesRead > 0)
                {
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    if (client.Available == 0)
                    {
                        string response = state.sb.ToString();

                        state.sb.Clear();

                        if( LoggingLevel == 1)
                        {
                            OnLogWriteEntry(EventLogEntryCodes.SocketClientDataReceived, new string[0]);
                        }
                        else if (LoggingLevel == 2)
                        {
                            OnLogWriteEntry(EventLogEntryCodes.SocketClientDataReceived, new string[] { response });
                        }

                        ReadEvent.Invoke(new Payload
                        (
                            ReadEvent.Id,
                            new PayloadSubject[] { new PayloadSubject(ReadEvent.Subjects[0], response ) }
                        ));
                    }

                    client.BeginReceive(state.buffer, 0, SocketState.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
            }
            catch (ObjectDisposedException theException)
            {
                ConnectionInError = true;
                // Connection Closed
                OnLogWriteEntry(EventLogEntryCodes.SocketClientObjectDisposedException, new string[] { "ReceiveCallback " + theException.Message });
                InitialiseAfterDelay();
            }
            catch (SocketException theSocketException)
            {
                OnSocketException(theSocketException);
            }
            catch (Exception theException)
            {
                ConnectionInError = true;

                if (!state.workSocket.Connected)
                {
                    InitialiseAfterDelay();
                }

                OnLogWriteEntry(EventLogEntryCodes.SocketClientException, new string[] { theException.Message });
            }
        }



        public bool Connected { get { return m_Socket == null ? false : m_Socket.Connected; } }

        public bool ConnectionInError
        {
            get { return m_ConnectionInError; }
            set
            {
                if( m_ConnectionInError != value)
                {
                    if(value)
                    {
                        ReadEvent.Enabled = false;

                        ReadEvent.Invoke(new Payload
                        (
                            ReadEvent.Id,
                            new PayloadSubject[0],
                            DateTime.Now.AddSeconds(2),
                            PayloadStatus.Disabled
                        ));
                    }
                    else
                    {
                        ReadEvent.Enabled = true;

                        ReadEvent.Invoke(new Payload
                        (
                            ReadEvent.Id,
                            new PayloadSubject[0],
                            DateTime.Now.AddSeconds(2),
                            PayloadStatus.Enabled
                        ));
                    }
                    m_ConnectionInError = value;
                }
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);

                if(LoggingLevel > 0)
                {
                    OnLogWriteEntry(EventLogEntryCodes.SocketClientDataSent, null);
                }

                ConnectionInError = false;
            }
            catch (SocketException ex)
            {
                OnSocketException(ex);
            }
            catch (Exception theException)
            {
                OnLogWriteEntry(EventLogEntryCodes.SocketClientException, new string[] { theException.Message });
            }
        }

        private bool Send(byte[] byteData)
        {
            bool Sent = true;

            try
            {
                m_Socket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), m_Socket);
            }
            catch (SocketException theException)
            {
                OnLogWriteEntry(EventLogEntryCodes.SocketClientSocketException, new string[] { theException.Message });
                Sent = false;
                OnSocketException(theException);
            }
            catch (Exception theException)
            {
                ConnectionInError = true;
                Sent = false;
                OnLogWriteEntry(EventLogEntryCodes.SocketClientException, new string[] { theException.Message });
            }

            return Sent;
        }

        private void OnSocketException(SocketException theException)
        {
            OnLogWriteEntry(EventLogEntryCodes.SocketClientSocketExceptionCode, new string[] { theException.SocketErrorCode.ToString() });
            ConnectionInError = true;
            InitialiseAfterDelay();
        }

        private void OnLogWriteEntry(EventLogEntryCodes theLogCode, string[] theArg)
        {
            if (theArg != null)
            {
                m_LoggingService.WriteEntry((uint)theLogCode, theArg);
            }
            else
            {
                m_LoggingService.WriteEntry((uint)theLogCode);
            }
        }
    }
}
