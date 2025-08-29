using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MultiPlug.Base.Exchange;
using MultiPlug.Base.Exchange.API;
using MultiPlug.Ext.Network.Sockets.Models.Components;
using MultiPlug.Ext.Network.Sockets.Diagnostics;
using MultiPlug.Ext.Network.Sockets.Components.Utils;
using MultiPlug.Ext.Network.Sockets.Models.Exchange;

namespace MultiPlug.Ext.Network.Sockets.Components.SocketEndpoint
{
    public class SocketEndpointComponent : SocketEndpointProperties
    {
        public event Action EventsUpdated;
        public event Action SubscriptionsUpdated;

        private TcpSocket m_Socket;

        private SocketEndpointListener m_Listener;
        private Thread m_ReadThread;

        readonly ILoggingService m_LoggingService;

        public string LogEventId { get { return m_LoggingService.EventId; } }

        private bool m_MultiPlugStarted = false;

        public SocketEndpointComponent(string theGuid, ILoggingService theLoggingService)
        {
            Guid = theGuid;
            m_LoggingService = theLoggingService;

            ReadEvent = new Event { Guid = theGuid, Id = System.Guid.NewGuid().ToString(), Description = "", Subjects = new[] { "value" }, Group = "Endpoint" };
            WriteSubscriptions = new WriteSubscription[0];

            m_Listener = new SocketEndpointListener(this);
            m_Listener.Log += OnLogWriteEntry;

            SubscriptionsControlConnect = true;

            AllowedList = new string[0];
            ReadTrim = false;
            LoggingShowControlCharacters = false;
        }

        internal new void Dispose()
        {
            try
            {
                // Bug Created https://github.com/British-Systems/MultiPlug/issues/52
                m_LoggingService.Delete();
            }
            catch (NotImplementedException)
            {
            }

            base.Dispose();
        }

        internal void UpdateProperties(SocketEndpointProperties theNewProperties)
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

                if (SubscriptionsControlConnect == false)
                {
                    ForceInitialise = true;
                }
            }

            if (theNewProperties.AllowedList != null)
            {
                AllowedList = theNewProperties.AllowedList
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Where(x =>
                    {
                        IPAddress address;
                        return System.Net.IPAddress.TryParse(x, out address);
                    }).ToArray();
            }

            if (theNewProperties.ReadEvent != null)
            {
                if (Event.Merge(ReadEvent, theNewProperties.ReadEvent))
                {
                    EventsUpdatedFlag = true;
                }
            }

            if (theNewProperties.ReadTrim != null && ReadTrim != theNewProperties.ReadTrim)
            {
                ReadTrim = theNewProperties.ReadTrim;
            }

            if (theNewProperties.ReadPrefix != null && ReadPrefix != theNewProperties.ReadPrefix)
            {
                ReadPrefix = theNewProperties.ReadPrefix;
            }

            if (theNewProperties.ReadAppend != null && ReadAppend != theNewProperties.ReadAppend)
            {
                ReadAppend = theNewProperties.ReadAppend;
            }

            if (theNewProperties.IPAddress != null && theNewProperties.IPAddress != IPAddress)
            {
                IPAddress = theNewProperties.IPAddress;
                ForceInitialise = true;
            }

            if (theNewProperties.Port != -1 && theNewProperties.Port != Port)
            {
                Port = theNewProperties.Port;
                ForceInitialise = true;
            }

            if (theNewProperties.Backlog != -1 && theNewProperties.Backlog != Backlog)
            {
                Backlog = theNewProperties.Backlog;
                ForceInitialise = true;
            }

            if (theNewProperties.LoggingLevel != -1 && theNewProperties.LoggingLevel != LoggingLevel)
            {
                LoggingLevel = theNewProperties.LoggingLevel;
            }

            if (theNewProperties.LoggingShowControlCharacters != null && theNewProperties.LoggingShowControlCharacters != LoggingShowControlCharacters)
            {
                LoggingShowControlCharacters = theNewProperties.LoggingShowControlCharacters;
            }

            if (theNewProperties.NICIndex != -1 && theNewProperties.NICIndex != NICIndex)
            {
                NICIndex = theNewProperties.NICIndex;
            }

            int NICIndexSearch = LocalIPAddressList.GetIndex(IPAddress);

            if (NICIndexSearch != -1)
            {
                NICIndex = NICIndexSearch;
            }

            if (theNewProperties.WriteSubscriptions != null)
            {
                List<WriteSubscription> NewSubscriptions = new List<WriteSubscription>();

                foreach (WriteSubscription Subscription in theNewProperties.WriteSubscriptions)
                {
                    WriteSubscription Search = WriteSubscriptions.FirstOrDefault(ne => ne.Guid == Subscription.Guid);

                    if (Search == null)
                    {
                        NewSubscriptions.Add(Subscription);
                    }
                    else
                    {
                        if (WriteSubscription.Merge(Search, Subscription))
                        {
                            SubscriptionsUpdatedFlag = true;
                        }
                    }
                }

                if (NewSubscriptions.Count() > 0)
                {
                    SubscriptionsUpdatedFlag = true;
                }

                foreach (WriteSubscription Subscription in NewSubscriptions)
                {
                    Subscription.Guid = string.IsNullOrEmpty(Subscription.Guid) ? System.Guid.NewGuid().ToString() : Subscription.Guid;
                    Subscription.WriteEvent += m_Listener.OnSubscriptionEvent;
                    Subscription.EnabledStatus += OnSubscriptionStatusChanged;
                }

                List<WriteSubscription> List = WriteSubscriptions == null ? new List<WriteSubscription>() : WriteSubscriptions.ToList();

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
                OnLogWriteEntry(EventLogEntryCodes.SocketEndpointSocketClosingDueToReconfigure, new string[0]);
                Shutdown();

                if (SubscriptionsControlConnect == true)
                {
                    OnSubscriptionStatusChanged(false);
                }
                else
                {
                    InitialiseSetup();
                }
            }
        }

        internal void DisconnectClient(string theClientGuidToDelete)
        {
            m_Listener.DisconnectClient(theClientGuidToDelete);
        }

        internal void Start(bool MultiPlugHasStarted)
        {
            m_MultiPlugStarted = MultiPlugHasStarted;
            InitialiseSetup();
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
            if (SubscriptionsControlConnect == true)
            {
                if (WriteSubscriptions.All(Subscription => Subscription.Enabled))
                {
                    if (!m_Listener.Listening)
                    {
                        InitialiseSetup();
                    }
                }
                else
                {
                    if (LoggingLevel > 0)
                    {
                        OnLogWriteEntry(EventLogEntryCodes.SocketEndpointSocketClosingDueToSubscriptionControl, new string[0]);
                    }

                    Shutdown();
                }
            }
        }

        internal ConnectedClient[] ConnectedClients()
        {
            return m_Listener.ConnectedClients();
        }

        internal void RemoveSubcription(string theSubcriptionGuid)
        {
            WriteSubscription Search = WriteSubscriptions.FirstOrDefault(s => s.Guid == theSubcriptionGuid);

            if (Search != null)
            {
                Search.WriteEvent -= m_Listener.OnSubscriptionEvent;
                Search.EnabledStatus -= OnSubscriptionStatusChanged;

                List<WriteSubscription> SubscriptionsList = WriteSubscriptions.ToList();
                SubscriptionsList.Remove(Search);
                WriteSubscriptions = SubscriptionsList.ToArray();

                SubscriptionsUpdated?.Invoke();
                OnSubscriptionStatusChanged(false); // Force a update with the new count of Subscriptions
            }
        }

        internal void Shutdown()
        {
            if (m_ReadThread != null && m_ReadThread.IsAlive)
            {
                m_ReadThread.Interrupt();
                m_ReadThread.Join();
                if (LoggingLevel > 0)
                {
                    OnLogWriteEntry(EventLogEntryCodes.SocketEndpointSocketClosed, new string[0]);
                }
            }
        }

        private void InitialiseSetup()
        {
            if (!m_MultiPlugStarted)
            {
                return;
            }

            if (string.IsNullOrEmpty(IPAddress))
            {
                OnLogWriteEntry(EventLogEntryCodes.NoIPAddress, null);
                return;
            }

            if (Port == 0)
            {
                OnLogWriteEntry(EventLogEntryCodes.IncorrectPort, null);
                return;
            }

            if (Backlog < 1)
            {
                OnLogWriteEntry(EventLogEntryCodes.IncorrectBacklogValue, null);
                return;
            }

            if (m_Socket != null)
            {
                Shutdown();
            }

            var IPAddressList = LocalIPAddressList.Get();

            // If the IPAddress has been updated, we attempt to update it using the NIC index

            if (!IPAddressList.Contains(IPAddress))
            {
                if (NICIndex < IPAddressList.Length)
                {
                    IPAddress = IPAddressList[NICIndex];
                    OnLogWriteEntry(EventLogEntryCodes.SocketEndpointLocalIPAddressUpdated, new string[] { IPAddress });
                }
                else
                {
                    OnLogWriteEntry(EventLogEntryCodes.SocketEndpointLocalIPAddressUpdateFailed, new string[] { IPAddress });
                    return;
                }
            }



            IPAddress ipAddr;

            if (!System.Net.IPAddress.TryParse(IPAddress, out ipAddr))
            {
                OnLogWriteEntry(EventLogEntryCodes.LocalIPAddressParse, null);
                return;
            }

            try
            {
                var ipEndPoint = new IPEndPoint(ipAddr, Port);

                m_Socket = new TcpSocket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                m_Socket.Bind(ipEndPoint);
                m_Socket.Listen(Backlog);

                m_Listener.Socket = m_Socket;

                m_ReadThread = new Thread(m_Listener.Listen);
                m_ReadThread.Name = "Thread for " + Guid;
                m_ReadThread.IsBackground = true;
                m_ReadThread.Start();
            }
            catch (Exception theException)
            {
                OnLogWriteEntry(EventLogEntryCodes.SocketBindException, new string[] { theException.Message });
            }
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

        internal void TerminalSend(string theWriteValue)
        {
            m_Listener.Send(Regex.Unescape(theWriteValue), false);
        }
    }
}
