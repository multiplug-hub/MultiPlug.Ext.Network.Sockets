using System;
using System.Linq;
using System.Collections.Generic;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Exchange;
using MultiPlug.Base.Http;
using MultiPlug.Ext.Network.Sockets.Components.SocketEndpoint;
using MultiPlug.Ext.Network.Sockets.Models.Settings;
using MultiPlug.Ext.Network.Sockets.Models.Components;

namespace MultiPlug.Ext.Network.Sockets.Controllers.Settings.SocketEndpoint
{
    [Route("socketendpoint")]
    public class SocketEndpointController : SettingsApp
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
                        Template = "NetworkSocketsItemNotFound"
                    };
                }
            }

            SocketEndpointProperties model;
            Subscription[] Subscriptions = null;

            if (SocketEndpoint != null)
            {
                model = SocketEndpoint;
                Subscriptions = new Subscription[] { new Subscription { Id = SocketEndpoint.LogEventId } };
            }
            else
            {
                string Guid = System.Guid.NewGuid().ToString();
                model = new SocketEndpointProperties
                {
                    ReadEvent = new Event { Guid = Guid, Id = Guid, Description = "Socket Read" }
                };
            }

            return new Response
            {
                Model = model,
                Template = "NetworkSocketsEndpointView",
                Subscriptions = Subscriptions
            };
        }

        public Response Post(SocketEndpointPostModel theModel)
        {
            if (theModel != null &&
                theModel.Guid != null &&
                theModel.EventId != null &&
                theModel.EventDescription != null &&
                theModel.EventKey != null &&
                theModel.IPAddress != null &&
                theModel.Port != null && 
                theModel.Backlog != null &&
                theModel.SubscriptionKey != null
                )
            {
                var Subscriptions = new List<Subscription>();

                if (theModel.SubscriptionGuid != null &&
                    theModel.SubscriptionId != null &&
                    theModel.SubscriptionGuid.Length == theModel.SubscriptionId.Length)
                {
                    for (int i = 0; i < theModel.SubscriptionGuid.Length; i++)
                    {
                        if (string.IsNullOrEmpty(theModel.SubscriptionId[i]))
                        {
                            continue;
                        }

                        Subscriptions.Add(new Subscription
                        {
                            Guid = (string.IsNullOrEmpty(theModel.SubscriptionGuid[i])) ? Guid.NewGuid().ToString() : theModel.SubscriptionGuid[i],
                            Id = theModel.SubscriptionId[i],
                        });
                    }
                }

                int Port = 0;

                int.TryParse(theModel.Port, out Port);

                int Backlog = 100;

                int.TryParse(theModel.Backlog, out Backlog);

                Core.Instance.Update(new SocketEndpointProperties[] {

                new SocketEndpointProperties
                {
                    ReadEvent = new Event { Guid = theModel.Guid, Id = theModel.EventId, Description = theModel.EventDescription },
                    WriteSubscriptions = Subscriptions,
                    IPAddress = theModel.IPAddress,
                    EventKey = theModel.EventKey,
                    SubscriptionKey = theModel.SubscriptionKey,
                    Port = Port,
                    Backlog = Backlog,
                    LoggingLevel = theModel.LoggingLevel
                } });

                return new Response
                {
                    StatusCode = System.Net.HttpStatusCode.Moved,
                    Location = new Uri(Context.Referrer, Context.Referrer.AbsolutePath + "?id=" + theModel.Guid)
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
