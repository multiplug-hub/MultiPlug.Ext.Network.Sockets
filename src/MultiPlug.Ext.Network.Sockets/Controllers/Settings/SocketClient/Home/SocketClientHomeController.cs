using System;
using System.Linq;
using System.Collections.Generic;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Exchange;
using MultiPlug.Base.Http;
using MultiPlug.Ext.Network.Sockets.Components.SocketClient;
using MultiPlug.Ext.Network.Sockets.Models.Components;
using MultiPlug.Ext.Network.Sockets.Models.Settings;
using MultiPlug.Ext.Network.Sockets.Models.Settings.SocketClient;

namespace MultiPlug.Ext.Network.Sockets.Controllers.Settings.SocketClient.Setup
{
    [Route("socketclient")]
    public class SocketClientHomeController : SettingsApp
    {
        public Response Get(string id)
        {
            SocketClientComponent SocketEndpoint = null;

            if ( ! string.IsNullOrEmpty(id))
            {
                SocketEndpoint = Core.Instance.SocketClients.FirstOrDefault(t => t.Guid == id);

                if( SocketEndpoint == null)
                {
                    return new Response
                    {
                        Template = Templates.SettingsNotFound
                    };
                }
            }

            SocketClientHomeModel Model;
            Subscription[] Subscriptions = null;

            if (SocketEndpoint != null)
            {
                Model = new SocketClientHomeModel
                {
                    Guid = SocketEndpoint.Guid,
                    LoggingLevel = SocketEndpoint.LoggingLevel,
                    TraceLog = SocketEndpoint.TraceLog,
                    Connected = SocketEndpoint.Connected,
                    ConnectionInError = SocketEndpoint.ConnectionInError
                };
                Subscriptions = new Subscription[] { new Subscription { Id = SocketEndpoint.LogEventId } };
            }
            else
            {
                string Guid = System.Guid.NewGuid().ToString();

                Model = new SocketClientHomeModel
                {
                    Guid = string.Empty,
                    LoggingLevel = 0,
                    TraceLog = string.Empty,
                    Connected = false,
                    ConnectionInError = false
                };

                Subscriptions = new Subscription[0];
            }

            return new Response
            {
                Model = Model,
                Template = Templates.SettingsSocketClientHome,
                Subscriptions = Subscriptions
            };
        }

        public Response Post(SocketClientHomeModel theModel)
        {
            if (theModel != null)
            {
                string EndpointGuid = string.IsNullOrEmpty(theModel.Guid) ? Guid.NewGuid().ToString() : theModel.Guid;

                Core.Instance.Update(new SocketClientProperties[] {
                    new SocketClientProperties
                    {
                        Guid = EndpointGuid,
                        LoggingLevel = theModel.LoggingLevel,
                    }
                });

                return new Response
                {
                    StatusCode = System.Net.HttpStatusCode.Moved,
                    Location = new Uri(Context.Referrer, Context.Referrer.AbsolutePath + "?id=" + EndpointGuid)
                };
            }

            return new Response
            {
                StatusCode = System.Net.HttpStatusCode.Moved,
                Location = Context.Referrer
            };
        }
    }
}
