using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MultiPlug.Base;
using MultiPlug.Base.Exchange;
using MultiPlug.Base.Exchange.API;
using MultiPlug.Ext.Network.Sockets.Models.Components;
using MultiPlug.Ext.Network.Sockets.Components.SocketClient;
using MultiPlug.Ext.Network.Sockets.Components.SocketEndpoint;

namespace MultiPlug.Ext.Network.Sockets
{
    internal class Core : MultiPlugBase
    {
        private static Core m_Instance = null;

        public event Action EventsUpdated;
        public event Action SubscriptionsUpdated;

        public Subscription[] Subscriptions { get; private set; } = new Subscription[0];
        public Event[] Events { get; private set; } = new Event[0];

        private bool m_MultiPlugStarted = false;

        public static Core Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new Core();
                }
                return m_Instance;
            }
        }

        [DataMember]
        public SocketEndpointComponent[] SocketEndpoints { get; set; } = new SocketEndpointComponent[0];
        [DataMember]
        public SocketClientComponent[] SocketClients { get; set; } = new SocketClientComponent[0];
        internal IMultiPlugServices MultiPlugServices { get; set; }

        private void OnEventsUpdated()
        {
            List<Event> EventsList = new List<Event>();
            Array.ForEach(SocketClients, s => EventsList.Add(s.ReadEvent));
            Array.ForEach(SocketEndpoints, s => EventsList.Add(s.ReadEvent));
            Events = EventsList.ToArray();
            EventsUpdated?.Invoke();
        }

        private void OnSubscriptionsUpdated()
        {
            List<Subscription> SubscriptionList = new List<Subscription>();
            Array.ForEach(SocketClients, s => SubscriptionList.AddRange(s.WriteSubscriptions));
            Array.ForEach(SocketEndpoints, s => SubscriptionList.AddRange(s.WriteSubscriptions));
            Subscriptions = SubscriptionList.ToArray();
            SubscriptionsUpdated?.Invoke();
        }

        internal void Add(SocketEndpointComponent[] theSocketClientComponents)
        {
            foreach (SocketEndpointComponent SocketClientComponent in theSocketClientComponents)
            {
                SocketClientComponent.SubscriptionsUpdated += OnSubscriptionsUpdated;
                SocketClientComponent.EventsUpdated += OnEventsUpdated;

                var SocketEndpointsList = new List<SocketEndpointComponent>(SocketEndpoints);
                SocketEndpointsList.Add(SocketClientComponent);
                SocketEndpoints = SocketEndpointsList.ToArray();
            }
        }

        internal void Remove(SocketEndpointComponent[] theSocketClientComponents)
        {
            foreach (SocketEndpointComponent SocketEndpointComponent in theSocketClientComponents)
            {
                SocketEndpointComponent.SubscriptionsUpdated -= OnSubscriptionsUpdated;
                SocketEndpointComponent.EventsUpdated -= OnEventsUpdated;
                SocketEndpointComponent.Shutdown();
                SocketEndpointComponent.Dispose();

                var SocketEndpointsList = new List<SocketEndpointComponent>(SocketEndpoints);
                SocketEndpointsList.Remove(SocketEndpointComponent);

                SocketEndpoints = SocketEndpointsList.ToArray();
            }

            if(theSocketClientComponents.Any())
            {
                OnSubscriptionsUpdated();
                OnEventsUpdated();
            }
        }

        internal void Start()
        {
            m_MultiPlugStarted = true;

            Array.ForEach(SocketEndpoints, s => s.Start(m_MultiPlugStarted));
            Array.ForEach(SocketClients, s => s.Start(m_MultiPlugStarted));
        }

        internal void Shutdown()
        {
            Array.ForEach(SocketEndpoints, s => s.Shutdown());
            Array.ForEach(SocketClients, s => s.Shutdown());
        }

        internal void Update(SocketEndpointProperties[] theSocketEndpointProperties)
        {
            foreach (SocketEndpointProperties SocketEndpointProperties in theSocketEndpointProperties)
            {
                SocketEndpointComponent SocketEndpoint = SocketEndpoints.FirstOrDefault(e => e.Guid == SocketEndpointProperties.Guid);

                if (SocketEndpoint != null)
                {
                    SocketEndpoint.UpdateProperties(SocketEndpointProperties);
                }
                else
                {
                    if(!string.IsNullOrEmpty(SocketEndpointProperties.Guid))
                    {
                        var Logger = MultiPlugServices.Logging.New(SocketEndpointProperties.Guid, Diagnostics.EventLogDefinitions.DefinitionsId);
                        SocketEndpoint = new SocketEndpointComponent(SocketEndpointProperties.Guid, Logger);
                        Add(new SocketEndpointComponent[] { SocketEndpoint });
                        SocketEndpoint.UpdateProperties(SocketEndpointProperties);
                        SocketEndpoint.Start(m_MultiPlugStarted);
                    }
                    
                }
            }
        }

        internal void RemoveSocketEndpointSubcription(string theSocketEndpointGuid, string theSubcriptionGuid)
        {
            SocketEndpointComponent SocketEndpoint = SocketEndpoints.FirstOrDefault(e => e.Guid == theSocketEndpointGuid);

            if (SocketEndpoint != null)
            {
                SocketEndpoint.RemoveSubcription(theSubcriptionGuid);
            }
        }

        internal void Add(SocketClientComponent[] theSocketClientComponents)
        {
            foreach (var item in theSocketClientComponents)
            {
                item.SubscriptionsUpdated += OnSubscriptionsUpdated;
                item.EventsUpdated += OnEventsUpdated;

                var SocketClientsAdd = new List<SocketClientComponent>(SocketClients);
                SocketClientsAdd.Add(item as SocketClientComponent);
                SocketClients = SocketClientsAdd.ToArray();
            }
        }

        internal void Remove(SocketClientComponent[] theSocketClientComponents)
        {
            foreach (SocketClientComponent SocketClientComponent in theSocketClientComponents)
            {
                SocketClientComponent.SubscriptionsUpdated -= OnSubscriptionsUpdated;
                SocketClientComponent.EventsUpdated -= OnEventsUpdated;
                SocketClientComponent.Shutdown();
                SocketClientComponent.Dispose();

                var SocketClientsList = new List<SocketClientComponent>(SocketClients);
                SocketClientsList.Remove(SocketClientComponent);

                SocketClients = SocketClientsList.ToArray();
            }

            if (theSocketClientComponents.Any())
            {
                OnSubscriptionsUpdated();
                OnEventsUpdated();
            }
        }

        internal void Update(SocketClientProperties[] theSocketClientProperties)
        {
            foreach (var item in theSocketClientProperties)
            {
                SocketClientComponent SocketClient = SocketClients.FirstOrDefault(c => c.Guid == item.Guid);

                if (SocketClient != null)
                {
                    SocketClient.UpdateProperties(item);
                }
                else
                {
                    if (!string.IsNullOrEmpty(item.Guid))
                    {
                        var Logger = MultiPlugServices.Logging.New(item.Guid, Diagnostics.EventLogDefinitions.DefinitionsId);
                        SocketClient = new SocketClientComponent(item.Guid, Logger);
                        Add(new SocketClientComponent[] { SocketClient });
                        SocketClient.UpdateProperties(item);
                        SocketClient.Start(m_MultiPlugStarted);
                    }
                }
            }
        }

        internal void RemoveSocketClientSubcription(string theSocketClientGuid, string theSubcriptionGuid)
        {
            SocketClientComponent SocketClient = SocketClients.FirstOrDefault(c => c.Guid == theSocketClientGuid);

            if (SocketClient != null)
            {
                SocketClient.RemoveSubcription(theSubcriptionGuid);
            }
        }

    }
}
