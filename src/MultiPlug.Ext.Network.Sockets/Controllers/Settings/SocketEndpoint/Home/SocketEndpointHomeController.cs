using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Exchange;
using MultiPlug.Base.Http;
using MultiPlug.Ext.Network.Sockets.Components.SocketEndpoint;
using MultiPlug.Ext.Network.Sockets.Models.Settings;
using MultiPlug.Ext.Network.Sockets.Models.Components;
using MultiPlug.Ext.Network.Sockets.Models.Settings.SocketEndpoint;
using MultiPlug.Ext.Network.Sockets.Components.Utils;

namespace MultiPlug.Ext.Network.Sockets.Controllers.Settings.SocketEndpoint.Home
{
    [Route("socketendpoint")]
    public class SocketEndpointHomeController : SettingsApp
    {
        public Response Get( string id )
        {
            SocketEndpointComponent SocketEndpoint = null;

            if ( ! string.IsNullOrEmpty(id))
            {
                SocketEndpoint = Core.Instance.SocketEndpoints.FirstOrDefault(t => t.Guid == id);

                if (SocketEndpoint == null)
                {
                    return new Response
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Template = Templates.SettingsNotFound
                    };
                }
            }

            SocketEndpointHomeModel model;
            Subscription[] Subscriptions = null;

            if (SocketEndpoint != null)
            {
                model = new SocketEndpointHomeModel
                {
                    Guid = SocketEndpoint.Guid,
                    ConnectedClients = SocketEndpoint.ConnectedClients(),
                    LoggingLevel = SocketEndpoint.LoggingLevel,
                    TraceLog = SocketEndpoint.TraceLog,
                    LoggingShowControlCharacters = SocketEndpoint.LoggingShowControlCharacters.Value
                };
                Subscriptions = new Subscription[] { new Subscription { Id = SocketEndpoint.LogEventId } };
            }
            else
            {
                model = new SocketEndpointHomeModel
                {
                    Guid = string.Empty,
                    ConnectedClients = new ConnectedClient[0],
                    LoggingLevel = 0,
                    TraceLog = string.Empty,
                    LoggingShowControlCharacters = false
                };
            }

            return new Response
            {
                Model = model,
                Template = Templates.SettingsSocketEndpointHome,
                Subscriptions = Subscriptions
            };
        }

        public Response Post(SocketEndpointHomeModel theModel)
        {
            if (theModel != null)
            {
                string EndpointGuid = string.IsNullOrEmpty(theModel.Guid) ? Guid.NewGuid().ToString() : theModel.Guid;

                Core.Instance.Update(new SocketEndpointProperties[] {
                    new SocketEndpointProperties
                    {
                        Guid = EndpointGuid,
                        LoggingLevel = theModel.LoggingLevel,
                        LoggingShowControlCharacters = theModel.LoggingShowControlCharacters

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
