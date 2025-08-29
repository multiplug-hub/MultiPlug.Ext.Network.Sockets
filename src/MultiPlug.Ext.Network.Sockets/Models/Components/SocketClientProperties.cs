using System.Runtime.Serialization;
using MultiPlug.Base;
using MultiPlug.Base.Exchange;
using MultiPlug.Ext.Network.Sockets.Models.Exchange;

namespace MultiPlug.Ext.Network.Sockets.Models.Components
{
    public class SocketClientProperties : MultiPlugBase
    {
        [DataMember]
        public string Guid { get; set; }

        [DataMember]
        public Event ReadEvent { get; set; }
        [DataMember]
        public WriteSubscription[] WriteSubscriptions { get; set; }
        [DataMember]
        public string HostName { get; set; }
        [DataMember]
        public int Port { get; set; } = -1;
        [DataMember]
        public int LoggingLevel { get; set; }
        [DataMember]
        public bool? SubscriptionsControlConnect { get; set; }
        //[DataMember] Future User Settable
        public bool? Enabled { get; set; }

        [DataMember]
        public bool? ReadTrim { get; set; }
        [DataMember]
        public string ReadPrefix { get; set; }
        [DataMember]
        public string ReadAppend { get; set; }
        [DataMember]
        public bool? LoggingShowControlCharacters { get; set; }

    }
}
