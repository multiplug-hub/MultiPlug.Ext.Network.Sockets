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
using MultiPlug.Ext.Network.Sockets.Models.Exchange;

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
                    ReadTrim = SocketEndpoint.ReadTrim.Value,
                    ReadPrefix = SocketEndpoint.ReadPrefix,
                    ReadAppend = SocketEndpoint.ReadAppend,

                    WriteSubscriptionGuids = SocketEndpoint.WriteSubscriptions.Select( s => s.Guid ).ToArray(),
                    WriteSubscriptionIds = SocketEndpoint.WriteSubscriptions.Select(s => s.Id ).ToArray(),
                    WriteSubscriptionConnecteds = SocketEndpoint.WriteSubscriptions.Select(s => s.Connected).ToArray(),

                    WriteSubscriptionWritePrefixs = SocketEndpoint.WriteSubscriptions.Select(x => x.WritePrefix).ToArray(),
                    WriteSubscriptionWriteSeparators = SocketEndpoint.WriteSubscriptions.Select(x => x.WriteSeparator).ToArray(),
                    WriteSubscriptionWriteSuffixs = SocketEndpoint.WriteSubscriptions.Select(x => x.WriteAppend).ToArray(),
                    WriteSubscriptionIsHexs = SocketEndpoint.WriteSubscriptions.Select(x => x.IsHex.Value).ToArray(),
                    WriteSubscriptionIgnoreDatas = SocketEndpoint.WriteSubscriptions.Select(x => x.IgnoreData.Value).ToArray(),


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
                    ReadTrim = false,
                    ReadPrefix = string.Empty,
                    ReadAppend = string.Empty,

                    WriteSubscriptionGuids = new string[0],
                    WriteSubscriptionIds = new string[0],
                    WriteSubscriptionConnecteds = new bool[0],
                    WriteSubscriptionWritePrefixs = new string[0],
                    WriteSubscriptionWriteSeparators = new string[0],
                    WriteSubscriptionWriteSuffixs = new string[0],
                    WriteSubscriptionIsHexs = new bool[0],
                    WriteSubscriptionIgnoreDatas = new bool[0],
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

        private bool[] RemoveDuplicates(int theParentLength, bool[] theArrayWithDuplicates)
        {
            bool[] TheResult = new bool[theParentLength];

            int index = 0;

            for (int i = 0; i < theArrayWithDuplicates.Length; i++)
            {
                if ((i + 1) < theArrayWithDuplicates.Length && theArrayWithDuplicates[i + 1])
                {
                    TheResult[index] = true;
                    i++;
                }
                else
                {
                    TheResult[index] = false;
                }
                index++;
            }

            return TheResult;
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

                var Subscriptions = new List<WriteSubscription>();

                if (theModel.WriteSubscriptionGuids != null)
                {
                    bool[] IsHexs = RemoveDuplicates(theModel.WriteSubscriptionGuids.Length, theModel.WriteSubscriptionIsHexs);
                    bool[] IgnoreDatas = RemoveDuplicates(theModel.WriteSubscriptionGuids.Length, theModel.WriteSubscriptionIgnoreDatas);

                    if (theModel.WriteSubscriptionGuids != null &&
                        theModel.WriteSubscriptionIds != null &&
                        theModel.WriteSubscriptionWritePrefixs != null &&
                        theModel.WriteSubscriptionWriteSeparators != null &&
                        theModel.WriteSubscriptionWriteSuffixs != null &&
                        theModel.WriteSubscriptionGuids.Length == theModel.WriteSubscriptionIds.Length &&
                        theModel.WriteSubscriptionGuids.Length == theModel.WriteSubscriptionWritePrefixs.Length &&
                        theModel.WriteSubscriptionGuids.Length == theModel.WriteSubscriptionWriteSeparators.Length &&
                        theModel.WriteSubscriptionGuids.Length == theModel.WriteSubscriptionWriteSuffixs.Length &&
                        theModel.WriteSubscriptionGuids.Length == IsHexs.Length &&
                        theModel.WriteSubscriptionGuids.Length == IgnoreDatas.Length)
                    {
                        for (int i = 0; i < theModel.WriteSubscriptionGuids.Length; i++)
                        {
                            if (string.IsNullOrEmpty(theModel.WriteSubscriptionIds[i]))
                            {
                                continue;
                            }

                            Subscriptions.Add(new WriteSubscription
                            {
                                Guid = (string.IsNullOrEmpty(theModel.WriteSubscriptionGuids[i])) ? Guid.NewGuid().ToString() : theModel.WriteSubscriptionGuids[i],
                                Id = theModel.WriteSubscriptionIds[i],
                                WritePrefix = theModel.WriteSubscriptionWritePrefixs[i],
                                WriteSeparator = theModel.WriteSubscriptionWriteSeparators[i],
                                WriteAppend = theModel.WriteSubscriptionWriteSuffixs[i],
                                IsHex = IsHexs[i],
                                IgnoreData = IgnoreDatas[i]
                            });
                        }
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
                    AllowedList = theModel.AllowedList == null ? new string[0] : theModel.AllowedList,
                    ReadTrim = theModel.ReadTrim,
                    ReadPrefix = theModel.ReadPrefix,
                    ReadAppend = theModel.ReadAppend
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
