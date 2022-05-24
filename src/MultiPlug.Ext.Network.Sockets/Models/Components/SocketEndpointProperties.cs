using System.Runtime.Serialization;
using MultiPlug.Base;
using MultiPlug.Base.Exchange;

namespace MultiPlug.Ext.Network.Sockets.Models.Components
{
    public class SocketEndpointProperties : MultiPlugBase
    {
        [DataMember]
        public string Guid { get; set; }

        [DataMember]
        public Event ReadEvent { get; set; }

        [DataMember]
        public Subscription[] WriteSubscriptions { get; set; }

        /// <summary>
        /// The maximum length of the pending connections queue.
        /// </summary>
        [DataMember]
        public int Backlog { get; set; } = -1;
        /// <summary>
        /// The port number associated with the address, or 0 to specify any available port.
        /// port is in host order.
        /// </summary>
        [DataMember]
        public int Port { get; set; } = -1;
        [DataMember]
        public string IPAddress { get; set; }
        [DataMember]
        public int NICIndex { get; set; } = -1;

        [DataMember]
        public int LoggingLevel { get; set; }
        [DataMember]
        public bool? SubscriptionsControlConnect { get; set; }

    }
}
