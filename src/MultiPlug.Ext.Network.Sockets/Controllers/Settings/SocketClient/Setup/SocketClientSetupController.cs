using System;
using System.Linq;
using System.Collections.Generic;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Exchange;
using MultiPlug.Base.Http;
using MultiPlug.Ext.Network.Sockets.Components.SocketClient;
using MultiPlug.Ext.Network.Sockets.Models.Components;
using MultiPlug.Ext.Network.Sockets.Models.Settings.SocketClient;

namespace MultiPlug.Ext.Network.Sockets.Controllers.Settings.SocketClient.Setup
{
    [Route("socketclient/setup")]
    public class SocketClientSetupController : SettingsApp
    {
        public Response Get(string id)
        {
            SocketClientComponent SocketClient = null;

            if ( ! string.IsNullOrEmpty(id))
            {
                SocketClient = Core.Instance.SocketClients.FirstOrDefault(Client => Client.Guid == id);

                if( SocketClient == null)
                {
                    return new Response
                    {
                        Template = Templates.SettingsNotFound
                    };
                }
            }

            SocketClientSetupModel model;
            Subscription[] Subscriptions = null;

            if (SocketClient != null)
            {
                model = new SocketClientSetupModel
                {
                    Guid = SocketClient.Guid,
                    HostName = SocketClient.HostName,
                    Port = SocketClient.Port,

                    ReadEventId = SocketClient.ReadEvent.Id,
                    ReadEventDescription = SocketClient.ReadEvent.Description,
                    ReadEventSubject = (SocketClient.ReadEvent.Subjects.Length > 0) ? SocketClient.ReadEvent.Subjects[0] : string.Empty,

                    WriteSubscriptionGuid = SocketClient.WriteSubscriptions.Select(s => s.Guid).ToArray(),
                    WriteSubscriptionId = SocketClient.WriteSubscriptions.Select(s => s.Id).ToArray(),
                    WriteSubscriptionConnected = SocketClient.WriteSubscriptions.Select(s => s.Connected).ToArray(),
                    SubscriptionsControlConnect = (SocketClient.SubscriptionsControlConnect == true)

                };
                Subscriptions = new Subscription[] { new Subscription { Id = SocketClient.LogEventId } };
            }
            else
            {
                string Guid = System.Guid.NewGuid().ToString();
                model = new SocketClientSetupModel
                {
                    Guid = string.Empty,
                    HostName = string.Empty,
                    Port = 0,

                    ReadEventId = Guid,
                    ReadEventDescription = "Socket Read",
                    ReadEventSubject = "value",

                    WriteSubscriptionGuid = new string[0],
                    WriteSubscriptionId = new string[0],
                    WriteSubscriptionConnected = new bool[0],
                    SubscriptionsControlConnect = true
                };

                Subscriptions = new Subscription[0];
            }

            return new Response
            {
                Model = model,
                Template = Templates.SettingsSocketClientSetup,
                Subscriptions = Subscriptions
            };
        }

        public Response Post(SocketClientSetupModel theModel)
        {
            if (theModel != null &&
                theModel.ReadEventId != null &&
                theModel.ReadEventDescription != null &&
                theModel.ReadEventSubject != null
                )
            {
                string EndpointGuid = string.IsNullOrEmpty(theModel.Guid) ? Guid.NewGuid().ToString() : theModel.Guid;

                var Subscriptions = new List<Subscription>();

                if (theModel.WriteSubscriptionGuid != null &&
                    theModel.WriteSubscriptionId != null &&
                    theModel.WriteSubscriptionGuid.Length == theModel.WriteSubscriptionId.Length)
                {
                    for (int i = 0; i < theModel.WriteSubscriptionGuid.Length; i++)
                    {
                        if (string.IsNullOrEmpty(theModel.WriteSubscriptionId[i]))
                        {
                            continue;
                        }

                        Subscriptions.Add(new Subscription
                        {
                            Guid = (string.IsNullOrEmpty(theModel.WriteSubscriptionGuid[i])) ? Guid.NewGuid().ToString() : theModel.WriteSubscriptionGuid[i],
                            Id = theModel.WriteSubscriptionId[i]
                        });
                    }
                }

                Core.Instance.Update(new SocketClientProperties[] {

                new SocketClientProperties
                {
                    Guid = EndpointGuid,
                    ReadEvent = new Event { Guid = EndpointGuid, Id = theModel.ReadEventId, Description = theModel.ReadEventDescription, Subjects = new string[] { theModel.ReadEventSubject } },
                    WriteSubscriptions = Subscriptions.ToArray(),
                    HostName = theModel.HostName,
                    Port = theModel.Port,
                    LoggingLevel = -1,
                    SubscriptionsControlConnect = theModel.SubscriptionsControlConnect
                } });

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
