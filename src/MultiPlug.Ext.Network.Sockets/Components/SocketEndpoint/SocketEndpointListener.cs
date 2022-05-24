using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using MultiPlug.Base.Exchange;
using MultiPlug.Ext.Network.Sockets.Models.Components;
using MultiPlug.Ext.Network.Sockets.Diagnostics;

namespace MultiPlug.Ext.Network.Sockets.Components.SocketEndpoint
{
    internal class SocketEndpointListener
    {
        private Socket m_Socket = null;
        readonly SocketEndpointProperties m_Properties;

        SocketState[] m_Sockets = new SocketState[0];

        private ManualResetEvent allDone = new ManualResetEvent(false);

        internal event Action<EventLogEntryCodes, string[]> Log;

        public SocketEndpointListener(SocketEndpointProperties theProperties)
        {
            m_Properties = theProperties;
            m_Properties.ReadEvent.Enabled = false;
        }

        internal Socket Socket
        {
            set
            {
                m_Socket = value;
            }
        }

        internal bool Listening
        {
            get
            {
                return m_Socket != null;
            }
        }

        internal string[] ConnectedClients()
        {
            var Sockets = m_Sockets;
            return Sockets.Select(s => s.Address).ToArray();
        }

        internal void Listen()
        {
            if (m_Socket == null) { return; }
            try
            {           
                while (true)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    if(m_Properties.LoggingLevel > 0)
                    {
                        Log?.Invoke(EventLogEntryCodes.WaitingForConnection, null);
                    }

                    m_Socket.BeginAccept( new AsyncCallback(AcceptCallback), m_Socket);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }
            }
            catch( ThreadInterruptedException)
            {
                Log?.Invoke(EventLogEntryCodes.SocketEndpointShutdown, null);
 
                m_Socket.Close();
                m_Socket = null;

                Array.ForEach(m_Sockets, s =>
                {
                   // s.workSocket.Disconnect(false);
                    s.workSocket.Shutdown(SocketShutdown.Both);
                    s.workSocket.Close();
                });
                m_Sockets = new SocketState[0];
                m_Properties.ReadEvent.Enabled = false;
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Socket listener = (Socket)ar.AsyncState;
                Socket client = listener.EndAccept(ar);

                if(m_Properties.LoggingLevel > 0)
                {
                    Log?.Invoke(EventLogEntryCodes.ConnectedTo, new string[] { client.RemoteEndPoint.ToString() });
                }

                // Signal the main thread to continue.  
                allDone.Set();

                // Create the state object.  
                SocketState state = new SocketState();
                state.workSocket = client;
                state.Address = (client.RemoteEndPoint as IPEndPoint).Address.ToString();

                var SocketList = new List<SocketState>(m_Sockets);
                SocketList.Add(state);
                m_Sockets = SocketList.ToArray();
                m_Properties.ReadEvent.Enabled = true;

                client.BeginReceive(state.buffer, 0, SocketState.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch( ObjectDisposedException theException)
            {
                Log?.Invoke(EventLogEntryCodes.SocketEndpointObjectDisposedException, new string[] { "AcceptCallback " + theException.Message });
            }
            catch( SocketException theSocketException)
            {
                Log?.Invoke(EventLogEntryCodes.SocketEndpointSocketException, new string[] { theSocketException.Message });
            }
            catch( Exception theException)
            {
                Log?.Invoke(EventLogEntryCodes.SocketEndpointException, new string[] { theException.Message });
            }
        }

        public void ReceiveCallback(IAsyncResult theAsyncResult)
        {
            SocketState state = (SocketState)theAsyncResult.AsyncState;

            try
            {
                Socket client = state.workSocket;

                if (!client.Connected)
                {
                    Log?.Invoke(EventLogEntryCodes.SocketEndpointClosedWhileReceive, new string[] { state.Address });
                    OnSocketException(state);
                    RemoveErroredSockets();
                    return;
                }

                // Read data from the client socket.   
                int bytesRead = client.EndReceive(theAsyncResult);

                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(
                        state.buffer, 0, bytesRead));

                    // Check for end-of-file tag. If it is not there, read   
                    // more data.  

                    if (client.Available == 0)
                    {
                        string response = state.sb.ToString();

                        state.sb.Clear();

                        if (m_Properties.LoggingLevel == 1)
                        {
                            Log?.Invoke(EventLogEntryCodes.SocketEndpointDataReceived, new string[] { string.Empty });
                        }
                        else if (m_Properties.LoggingLevel == 2)
                        {
                            Log?.Invoke(EventLogEntryCodes.SocketEndpointDataReceived, new string[] { response });
                        }

                        m_Properties.ReadEvent.Invoke(new Payload
                        (
                            m_Properties.ReadEvent.Id,
                            new PayloadSubject[] { new PayloadSubject(m_Properties.ReadEvent.Subjects[0], response ) }
                        ));
                    }

                    client.BeginReceive(state.buffer, 0, SocketState.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
                }
            }
            catch (ObjectDisposedException theException)
            {
                state.Errored = true;
                Log?.Invoke(EventLogEntryCodes.SocketEndpointObjectDisposedException, new string[] { "ReceiveCallback " + theException.Message });
                RemoveErroredSockets();
            }
            catch ( SocketException theSocketException)
            {
                OnSocketException(state);
                RemoveErroredSockets();

                if ( theSocketException.SocketErrorCode == SocketError.ConnectionReset)
                {
                    Log?.Invoke(EventLogEntryCodes.SocketEndpointExceptionConnectionReset, new string[] { state.Address });
                }
                else
                {
                    Log?.Invoke(EventLogEntryCodes.SocketEndpointException, new string[] { theSocketException.Message });
                }

                m_Properties.ReadEvent.Invoke(new Payload
                (
                    m_Properties.ReadEvent.Id,
                    new PayloadSubject[0],
                    DateTime.Now.AddSeconds(2),
                    PayloadStatus.Disabled
                ));
            }
            catch (Exception theSocketException)
            {
                if( ! state.workSocket.Connected )
                {
                    OnSocketException(state);
                    RemoveErroredSockets();
                }

                Log?.Invoke(EventLogEntryCodes.SocketEndpointException, new string[] { theSocketException.Message });
            }
        }

        internal void Send(string data)
        {
            if (m_Properties.LoggingLevel == 1)
            {
                Log?.Invoke(EventLogEntryCodes.SocketEndpointSending, new string[] { string.Empty });
            }
            else if (m_Properties.LoggingLevel == 2)
            {
                Log?.Invoke(EventLogEntryCodes.SocketEndpointSending, new string[] { data });
            }

            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            var Sockets = m_Sockets;

            foreach ( SocketState SocketState in Sockets)
            {
                if( SocketState.Errored)
                {
                    continue;
                }

                try
                {
                    SocketState.workSocket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), SocketState);
                }
                catch (SocketException ex)
                {
                    OnSocketException(SocketState);

                    Log?.Invoke(EventLogEntryCodes.SocketEndpointExceptionDisconnected, new string[] { SocketState.workSocket.RemoteEndPoint.ToString(), ex.SocketErrorCode.ToString() });

                    m_Properties.ReadEvent.Invoke(new Payload
                    (
                        m_Properties.ReadEvent.Id,
                        new PayloadSubject[0],
                        DateTime.Now.AddSeconds(2),
                        PayloadStatus.Disabled
                    ));
                    RemoveErroredSockets();
                }
            }
        }

        private void SendCallback(IAsyncResult theAsyncResult)
        {
            SocketState SocketState = (SocketState)theAsyncResult.AsyncState;

            try
            {
                // Complete sending the data to the remote device.  
                int bytesSent = SocketState.workSocket.EndSend(theAsyncResult);

                if(m_Properties.LoggingLevel > 0)
                {
                    Log?.Invoke(EventLogEntryCodes.SocketEndpointSent, new string[] { bytesSent.ToString() });
                }
            }
            catch( SocketException theSocketException)
            {
                Log?.Invoke(EventLogEntryCodes.SocketEndpointSocketException, new string[] { theSocketException.Message });
                OnSocketException(SocketState);
                RemoveErroredSockets();
            }
            catch (Exception theException)
            {
                Log?.Invoke(EventLogEntryCodes.SocketEndpointException, new string[] { theException.Message });
            }
        }

        private void OnSocketException( SocketState theState )
        {
            theState.Errored = true;
        }

        private void RemoveErroredSockets()
        {
            m_Sockets = m_Sockets.Where(s => s.Errored == false).ToArray();

            if (m_Sockets.Length == 0)
            {
                m_Properties.ReadEvent.Enabled = false;
            }
        }

        public void OnSubscriptionEvent(SubscriptionEvent theSubscriptionEvent)
        {
            foreach( var Subject in theSubscriptionEvent.PayloadSubjects)
            {
                Send(Subject.Value);
            }
        }
    }
}
