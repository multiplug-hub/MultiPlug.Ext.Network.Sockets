using System;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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

        

        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);

        private MessageBuffer m_MessageBuffer;
        private EventConsumerHelper m_EventConsumerHelper;

        private Socket m_Socket;

        private bool m_ConnectionInError = false;

        public string LogEventId { get { return m_LoggingService.EventId; } }

        public SocketClientComponent( string theGuid, ILoggingService theLoggingService)
        {
            ReadEvent = new Event { Guid = theGuid, Id = System.Guid.NewGuid().ToString(), Description = "" };
            m_LoggingService = theLoggingService;


            m_MessageBuffer = new MessageBuffer(theGuid, "SocketClient");

            m_EventConsumerHelper = new EventConsumerHelper(this, OnLogWriteEntry, Send, m_MessageBuffer);
        }
        internal void UpdateProperties(SocketClientProperties theNewProperties)
        {
            bool SubscriptionsUpdatedFlag = false;
            bool EventsUpdatedFlag = false;

            bool ForceInitialise = false;

            if (theNewProperties.ReadEvent.Guid != ReadEvent.Guid)
                return;

            if (theNewProperties.ReadEvent.Description != ReadEvent.Description)
            {
                ReadEvent.Description = theNewProperties.ReadEvent.Description;
            }

            if (theNewProperties.ReadEvent.Id != ReadEvent.Id)
            {
                ReadEvent.Id = theNewProperties.ReadEvent.Id;
                EventsUpdatedFlag = true;
            }

            if (theNewProperties.HostName != HostName)
            {
                HostName = theNewProperties.HostName;
                ForceInitialise = true;
            }
            if (theNewProperties.Port != Port)
            {
                Port = theNewProperties.Port;
                ForceInitialise = true;
            }

            EventKey = theNewProperties.EventKey;
            SubscriptionKey = theNewProperties.SubscriptionKey;

            if (theNewProperties.WriteSubscriptions != null)
            {
                List<Subscription> NewSubscriptions = new List<Subscription>();

                foreach ( Subscription Subscription in theNewProperties.WriteSubscriptions)
                {
                    Subscription Search = WriteSubscriptions.Find(ne => ne.Guid == Subscription.Guid);

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
                    Subscription.EventConsumer = m_EventConsumerHelper;
                }

                WriteSubscriptions.AddRange(NewSubscriptions);
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
                InitialiseSetup();
            }
        }

        internal void RemoveSubcription(string theSubcriptionGuid)
        {
            Subscription Search = WriteSubscriptions.FirstOrDefault(s => s.Guid == theSubcriptionGuid);

            if (Search != null)
            {
                WriteSubscriptions.Remove(Search);
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
                }

                    m_Socket.Close();
                }
                catch(Exception)
                {

                }
            }
        }

        private void InitialiseSetup()
        {
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
                m_EventConsumerHelper.Socket = m_Socket;

                OnLogWriteEntry(EventLogEntryCodes.SocketClientConnectingTo, new string[] { IPAddress.ToString() });
                 
                m_Socket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), m_Socket);
            }
            catch (Exception theException)
            {
                OnLogWriteEntry(EventLogEntryCodes.SocketClientException, new string[] { theException.Message });
            }
        }

        private void InitialiseAfterDelay()
        {
            var timer = new System.Timers.Timer(2000);
            timer.AutoReset = false;
            timer.Elapsed += (s, e) => { InitialiseSetup(); };
            timer.Start();
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            { 
                Socket client = (Socket)ar.AsyncState;

                client.EndConnect(ar);

                OnLogWriteEntry(EventLogEntryCodes.ConnectedTo, new string[] { client.RemoteEndPoint.ToString() });

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
            catch (ObjectDisposedException)
            {
                // Connection Closed
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

        private void ReceiveCallback(IAsyncResult ar)
        {
            SocketState state = (SocketState)ar.AsyncState;

            try
            {
                Socket client = state.workSocket; 
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    if (client.Available == 0)
                    {
                        string response = state.sb.ToString();

                        state.sb.Clear();

                        OnLogWriteEntry(EventLogEntryCodes.SocketClientReceivedData, new string[] { response });

                        ReadEvent.Fire(new Payload
                        (
                            ReadEvent.Id,
                            new PayloadSubject[] { new PayloadSubject( EventKey, response ) }
                        ));
                    }

                    client.BeginReceive(state.buffer, 0, SocketState.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
            }
            catch (ObjectDisposedException)
            {
                ConnectionInError = true;
                // Connection Closed
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

                        ReadEvent.Fire(new Payload
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

                        ReadEvent.Fire(new Payload
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
                OnLogWriteEntry(EventLogEntryCodes.SocketClientDataSent, null);

                ConnectionInError = false;

                // Signal that all bytes have been sent.  
                sendDone.Set();
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
                sendDone.WaitOne();
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

        /// <summary>
        /// Temporary: Until Subscription EventConsumer has been made a Interface this Helper Class will have to exist
        /// </summary>
        private class EventConsumerHelper : EventConsumer
        {
            private SocketClientProperties Properties;

            public Socket Socket { get; set; }
            private Action<EventLogEntryCodes, string[]> OnLogWriteEntry;
            private Func<byte[], bool> Send;
            private MessageBuffer m_MessageBuffer;

            public EventConsumerHelper(SocketClientProperties theProperties,
                Action<EventLogEntryCodes, string[]> theLog,
                Func<byte[], bool> theSendFunc,
                MessageBuffer theMessageBuffer)
            {
                Properties = theProperties;
                OnLogWriteEntry = theLog;
                Send = theSendFunc;
                m_MessageBuffer = theMessageBuffer;
            }

            public override void OnEvent(Payload thePayload)
            {
                PayloadSubject Subject = thePayload.Subjects.FirstOrDefault(p => p.Subject.Equals(Properties.SubscriptionKey, StringComparison.OrdinalIgnoreCase));

                if (Subject != null)
                {
                    if (Socket != null && Socket.Connected)
                    {
                        OnLogWriteEntry(EventLogEntryCodes.SocketClientSending, new string[] { Subject.Value });
                        Send(Encoding.ASCII.GetBytes(Subject.Value));
                    }
                    else
                    {
                        m_MessageBuffer.Enqueue(Subject.Value, thePayload.TimeToLive);
                    }
                }
            }
        }
    }
}
