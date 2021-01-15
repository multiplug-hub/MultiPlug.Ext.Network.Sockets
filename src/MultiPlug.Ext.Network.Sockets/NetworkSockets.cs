using System.Linq;
using System.Collections.Generic;
using MultiPlug.Base.Exchange;
using MultiPlug.Extension.Core;
using MultiPlug.Extension.Core.Http;
using MultiPlug.Ext.Network.Sockets.Models.Load;
using MultiPlug.Ext.Network.Sockets.Properties;

namespace MultiPlug.Ext.Network.Sockets
{
    public class NetworkSockets : MultiPlugExtension
    {
        private List<LoadModel> m_Load = new List<LoadModel>();

        public NetworkSockets()
        {
            Core.Instance.SubscriptionsUpdated += OnSubscriptionsUpdated;
            Core.Instance.EventsUpdated += OnEventsUpdated;

            MultiPlugServices.Logging.RegisterDefinitions(Diagnostics.EventLogDefinitions.DefinitionsId, Diagnostics.EventLogDefinitions.Definitions, true);

            Core.Instance.MultiPlugServices = MultiPlugServices;
        }

        private void OnEventsUpdated()
        {
            MultiPlugActions.Extension.Updates.Events();
        }

        public override Event[] Events
        {
            get
            {
                return Core.Instance.Events;
            }
        }

        private void OnSubscriptionsUpdated()
        {
            MultiPlugActions.Extension.Updates.Subscriptions();
        }

        public override Subscription[] Subscriptions
        {
            get
            {
                return Core.Instance.Subscriptions;
            }
        }

        public override RazorTemplate[] RazorTemplates
        {
            get
            {
                return new RazorTemplate[]
                {
                    new RazorTemplate("NetworkSocketsHomeView", Resources.HomeRazor),
                    new RazorTemplate("NetworkSocketsClientView", Resources.SocketClientRazor),
                    new RazorTemplate("NetworkSocketsEndpointView", Resources.SocketEndpointRazor),
                    new RazorTemplate("NetworkSocketsItemNotFound", Resources.NotFound),
                };
            }
        }

        public void Load(LoadModel config)
        {
            m_Load.Add(config);
        }

        public override void Initialise()
        {
            LoadModel[] Load = m_Load.ToArray();
            m_Load.Clear();

            foreach( LoadModel LoadModel in Load )
            {
                if (LoadModel.SocketClients != null)
                {
                    Core.Instance.Update(LoadModel.SocketClients);
                }

                if (LoadModel.SocketEndpoints != null)
                {
                    Core.Instance.Update(LoadModel.SocketEndpoints);
                }
            }
        }

        public override object Save()
        {
            return Core.Instance;
        }

        public override void Shutdown()
        {
            Core.Instance.Shutdown();
        }
    }
}
