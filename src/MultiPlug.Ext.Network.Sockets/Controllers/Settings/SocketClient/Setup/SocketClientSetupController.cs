using System;
using System.Linq;
using System.Collections.Generic;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Exchange;
using MultiPlug.Base.Http;
using MultiPlug.Ext.Network.Sockets.Components.SocketClient;
using MultiPlug.Ext.Network.Sockets.Models.Components;
using MultiPlug.Ext.Network.Sockets.Models.Settings.SocketClient;
using MultiPlug.Ext.Network.Sockets.Models.Exchange;

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
                    ReadTrim = SocketClient.ReadTrim.Value,
                    ReadPrefix = SocketClient.ReadPrefix,
                    ReadAppend = SocketClient.ReadAppend,

                    WriteSubscriptionGuids = SocketClient.WriteSubscriptions.Select(s => s.Guid).ToArray(),
                    WriteSubscriptionIds = SocketClient.WriteSubscriptions.Select(s => s.Id).ToArray(),
                    WriteSubscriptionConnecteds = SocketClient.WriteSubscriptions.Select(s => s.Connected).ToArray(),

                    WriteSubscriptionWritePrefixs = SocketClient.WriteSubscriptions.Select(x => x.WritePrefix).ToArray(),
                    WriteSubscriptionWriteSeparators = SocketClient.WriteSubscriptions.Select(x => x.WriteSeparator).ToArray(),
                    WriteSubscriptionWriteSuffixs = SocketClient.WriteSubscriptions.Select(x => x.WriteAppend).ToArray(),
                    WriteSubscriptionIsHexs = SocketClient.WriteSubscriptions.Select(x => x.IsHex.Value).ToArray(),
                    WriteSubscriptionIgnoreDatas = SocketClient.WriteSubscriptions.Select(x => x.IgnoreData.Value).ToArray(),

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

        public Response Post(SocketClientSetupModel theModel)
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

                Core.Instance.Update(new SocketClientProperties[] {

                new SocketClientProperties
                {
                    Guid = EndpointGuid,
                    ReadEvent = new Event { Guid = EndpointGuid, Id = theModel.ReadEventId, Description = theModel.ReadEventDescription, Subjects = new string[] { theModel.ReadEventSubject } },
                    WriteSubscriptions = Subscriptions.ToArray(),
                    HostName = theModel.HostName,
                    Port = theModel.Port,
                    LoggingLevel = -1,
                    SubscriptionsControlConnect = theModel.SubscriptionsControlConnect,
                    ReadTrim = theModel.ReadTrim,
                    ReadPrefix = theModel.ReadPrefix,
                    ReadAppend = theModel.ReadAppend
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
