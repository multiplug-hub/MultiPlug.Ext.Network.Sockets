using System;
using System.Linq;
using System.Collections.Generic;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Exchange;
using MultiPlug.Base.Http;
using MultiPlug.Ext.Network.Sockets.Components.SocketClient;
using MultiPlug.Ext.Network.Sockets.Models.Components;
using MultiPlug.Ext.Network.Sockets.Models.Settings;

namespace MultiPlug.Ext.Network.Sockets.Controllers.Settings.SocketClient
{
    [Route("socketclient")]
    public class SocketClientController : SettingsApp
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
                        Template = "NetworkSocketsItemNotFound"
                    };
                }
            }

            SocketClientProperties model;
            Subscription[] Subscriptions = null;

            if (SocketEndpoint != null)
            {
                model = SocketEndpoint;
                Subscriptions = new Subscription[] { new Subscription { Id = SocketEndpoint.LogEventId } };
            }
            else
            {
                String Guid = System.Guid.NewGuid().ToString();
                model = new SocketClientProperties
                {
                    ReadEvent = new Event { Guid = Guid, Id = Guid, Description = "Socket Read" }
                };

                Subscriptions = new Subscription[0];
            }

            return new Response
            {
                Model = model,
                Template = "NetworkSocketsClientView",
                Subscriptions = Subscriptions
            };
        }

        public Response Post(SocketClientPostModel theModel)
        {
            if (theModel != null &&
                theModel.Guid != null &&
                theModel.EventId != null &&
                theModel.EventDescription != null &&
                theModel.EventKey != null &&
                theModel.HostName != null &&
                theModel.Port != null && 
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

                Core.Instance.Update(new SocketClientProperties[] {

                new SocketClientProperties
                {
                    ReadEvent = new Event { Guid = theModel.Guid, Id = theModel.EventId, Description = theModel.EventDescription },
                    WriteSubscriptions = Subscriptions, 
                    HostName = theModel.HostName,
                    EventKey = theModel.EventKey,
                    SubscriptionKey = theModel.SubscriptionKey,
                    Port = Port,
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
