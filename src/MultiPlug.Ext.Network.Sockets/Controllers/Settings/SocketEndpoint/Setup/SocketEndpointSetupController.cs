﻿using System;
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

namespace MultiPlug.Ext.Network.Sockets.Controllers.Settings.SocketEndpoint.Setup
{
    [Route("socketendpoint/setup")]
    public class SocketEndpointSetupController : SettingsApp
    {
        public Response Get(string id)
        {
            SocketEndpointComponent SocketEndpoint = null;

            if (!string.IsNullOrEmpty(id))
            {
                SocketEndpoint = Core.Instance.SocketEndpoints.FirstOrDefault(Endpoint => Endpoint.Guid == id);

                if (SocketEndpoint == null)
                {
                    return new Response
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Template = Templates.SettingsNotFound
                    };
                }
            }

            SocketEndpointSetupModel model;
            Subscription[] Subscriptions = null;

            if (SocketEndpoint != null)
            {
                model = new SocketEndpointSetupModel
                {
                    Guid = SocketEndpoint.Guid,
                    IPAddressList = LocalIPAddressList.Get(),
                    Port = SocketEndpoint.Port,
                    Backlog = SocketEndpoint.Backlog,
                    NICIndex = SocketEndpoint.NICIndex,

                    ReadEventId = SocketEndpoint.ReadEvent.Id,
                    ReadEventDescription = SocketEndpoint.ReadEvent.Description,
                    ReadEventSubject = (SocketEndpoint.ReadEvent.Subjects.Length > 0 ) ? SocketEndpoint.ReadEvent.Subjects[0] : string.Empty,

                    WriteSubscriptionGuid = SocketEndpoint.WriteSubscriptions.Select( s => s.Guid ).ToArray(),
                    WriteSubscriptionId = SocketEndpoint.WriteSubscriptions.Select(s => s.Id ).ToArray(),
                    WriteSubscriptionConnected = SocketEndpoint.WriteSubscriptions.Select(s => s.Connected).ToArray(),
                    SubscriptionsControlConnect = ( SocketEndpoint.SubscriptionsControlConnect == true ),
                    AllowedList = SocketEndpoint.AllowedList

                };
                Subscriptions = new Subscription[] { new Subscription { Id = SocketEndpoint.LogEventId } };
            }
            else
            {
                string Guid = System.Guid.NewGuid().ToString();

                model = new SocketEndpointSetupModel
                {
                    Guid = string.Empty,
                    IPAddressList = LocalIPAddressList.Get(),
                    Port = 0,
                    Backlog = 100,
                    NICIndex = 0,

                    ReadEventId = Guid,
                    ReadEventDescription = "Socket Read",
                    ReadEventSubject = "value",

                    WriteSubscriptionGuid = new string[0],
                    WriteSubscriptionId = new string[0],
                    WriteSubscriptionConnected = new bool[0],
                    SubscriptionsControlConnect = true,
                    AllowedList = new string[0]
                };
            }

            return new Response
            {
                Model = model,
                Template = Templates.SettingsSocketEndpointSetup,
                Subscriptions = Subscriptions
            };
        }

        public Response Post(SocketEndpointSetupModel theModel)
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

                Core.Instance.Update(new SocketEndpointProperties[] {

                new SocketEndpointProperties
                {
                    Guid = EndpointGuid,
                    ReadEvent = new Event { Guid = EndpointGuid, Id = theModel.ReadEventId, Description = theModel.ReadEventDescription, Subjects = new string[] { theModel.ReadEventSubject } },
                    WriteSubscriptions = Subscriptions.ToArray(),
                    IPAddress = LocalIPAddressList.Get()[theModel.NICIndex],
                    NICIndex = theModel.NICIndex,
                    Port = theModel.Port,
                    Backlog = theModel.Backlog,
                    LoggingLevel = -1,
                    SubscriptionsControlConnect = theModel.SubscriptionsControlConnect,
                    AllowedList = theModel.AllowedList == null ? new string[0] : theModel.AllowedList
                } });

                return new Response
                {
                    StatusCode = HttpStatusCode.Moved,
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
