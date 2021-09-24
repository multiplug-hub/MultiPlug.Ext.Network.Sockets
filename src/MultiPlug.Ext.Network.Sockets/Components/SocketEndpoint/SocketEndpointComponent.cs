using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using MultiPlug.Base.Exchange;
using MultiPlug.Base.Exchange.API;
using MultiPlug.Ext.Network.Sockets.Models.Components;
using MultiPlug.Ext.Network.Sockets.Diagnostics;
using MultiPlug.Ext.Network.Sockets.Components.Utils;

namespace MultiPlug.Ext.Network.Sockets.Components.SocketEndpoint
{
    public class SocketEndpointComponent : SocketEndpointProperties
    {
        public event Action EventsUpdated;
        public event Action SubscriptionsUpdated;

        private Socket m_Socket;

        private SocketEndpointListener m_Listener;
        private Thread m_ReadThread;

        readonly ILoggingService m_LoggingService;

        public string LogEventId { get { return m_LoggingService.EventId; } }

        public SocketEndpointComponent(string theGuid, ILoggingService theLoggingService)
        {
            Guid = theGuid;
            m_LoggingService = theLoggingService;

            ReadEvent = new Event { Guid = theGuid, Id = System.Guid.NewGuid().ToString(), Description = "", Subjects = new[] { "value" } };
            WriteSubscriptions = new Subscription[0];

            m_Listener = new SocketEndpointListener(this);
            m_Listener.Log += OnLogWriteEntry;

            m_ReadThread = new Thread(m_Listener.Listen);
            m_ReadThread.Name = "Thread for " + theGuid;
            m_ReadThread.IsBackground = true;
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

            if (theNewProperties.ReadEvent != null)
            {
                if ( Event.Merge(ReadEvent, theNewProperties.ReadEvent) )
                {
                    EventsUpdatedFlag = true;
                }
            }

            if (theNewProperties.IPAddress != null && theNewProperties.IPAddress != IPAddress )
            {
                IPAddress = theNewProperties.IPAddress;
                ForceInitialise = true;
            }

            if(theNewProperties.Port != -1 && theNewProperties.Port != Port )
            {
                Port = theNewProperties.Port;
                ForceInitialise = true;
            }

            if(theNewProperties.Backlog != -1 && theNewProperties.Backlog != Backlog )
            {
                Backlog = theNewProperties.Backlog;
                ForceInitialise = true;
            }

            if( theNewProperties.LoggingLevel != -1 && theNewProperties.LoggingLevel != LoggingLevel )
            {
                LoggingLevel = theNewProperties.LoggingLevel;
            }

            if(theNewProperties.NICIndex != -1 && theNewProperties.NICIndex != NICIndex)
            {
                NICIndex = theNewProperties.NICIndex;
            }

            int NICIndexSearch = LocalIPAddressList.GetIndex(IPAddress);

            if(NICIndexSearch != -1)
            {
                NICIndex = NICIndexSearch;
            }

            if (theNewProperties.WriteSubscriptions != null)
            {
                List<Subscription> NewSubscriptions = new List<Subscription>();

                foreach (Subscription Subscription in theNewProperties.WriteSubscriptions)
                {
                    Subscription Search = WriteSubscriptions.FirstOrDefault(ne => ne.Guid == Subscription.Guid);

                    if (Search == null)
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
                    Subscription.Event += m_Listener.OnSubscriptionEvent;
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

            if( ForceInitialise )
            {
                InitialiseSetup();
            }
        }

        public string[] ConnectedClients()
        {
            return m_Listener.ConnectedClients();
        }

        internal void RemoveSubcription(string theSubcriptionGuid)
        {
            Subscription Search = WriteSubscriptions.FirstOrDefault(s => s.Guid == theSubcriptionGuid);

            if (Search != null)
            {
                var list = WriteSubscriptions.ToList();
                list.Remove(Search);
                WriteSubscriptions = list.ToArray();

                SubscriptionsUpdated?.Invoke();
            }
        }

        internal void Shutdown()
        {
            if (m_ReadThread.IsAlive)
            {
                m_ReadThread.Interrupt();
                m_ReadThread.Join();
            }
        }

        private void InitialiseSetup()
        {
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

            if( ! IPAddressList.Contains(IPAddress) )
            {
                if(NICIndex < IPAddressList.Length)
                {
                    IPAddress = IPAddressList[NICIndex];
                    OnLogWriteEntry(EventLogEntryCodes.LocalIPAddressUpdated, new string[] { IPAddress });
                }
                else
                {
                    OnLogWriteEntry(EventLogEntryCodes.LocalIPAddressUpdateFailed, new string[] { IPAddress });
                    return;
                }
            }



            IPAddress ipAddr;

            if ( !System.Net.IPAddress.TryParse( IPAddress, out ipAddr) )
            {
                OnLogWriteEntry(EventLogEntryCodes.LocalIPAddressParse, null);
                return;
            }

            try
            {
                var ipEndPoint = new IPEndPoint(ipAddr, Port);

                m_Socket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                m_Socket.Bind(ipEndPoint);
                m_Socket.Listen(Backlog);

                m_Listener.Socket = m_Socket;
         
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
    }
}
