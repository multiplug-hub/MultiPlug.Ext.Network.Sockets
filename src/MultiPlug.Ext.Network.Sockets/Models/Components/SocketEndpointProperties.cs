using System.Net;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MultiPlug.Base;
using MultiPlug.Base.Exchange;

namespace MultiPlug.Ext.Network.Sockets.Models.Components
{
    public class SocketEndpointProperties : MultiPlugBase
    {
        public string Guid { get { return ReadEvent.Guid; } }

        [DataMember]
        public Event ReadEvent { get; set; }

        [DataMember]
        public List<Subscription> WriteSubscriptions { get; set; } = new List<Subscription>();

        /// <summary>
        /// The maximum length of the pending connections queue.
        /// </summary>
        [DataMember]
        public int Backlog { get; set; } = 100;
        /// <summary>
        /// The port number associated with the address, or 0 to specify any available port.
        /// port is in host order.
        /// </summary>
        [DataMember]
        public int Port { get; set; } = 0;
        [DataMember]
        public string IPAddress { get; set; } = string.Empty;
        [DataMember]
        public string EventKey { get; set; } = "value";
        [DataMember]
        public string SubscriptionKey { get; set; } = "value";

        public string[] IPAddressList
        {
            get
            {
                IPHostEntry ipHost = Dns.GetHostEntry("");
                return ipHost.AddressList.Select( a => a.ToString() ).ToArray();
            }
        }

        public string TraceLog
        {
            get
            {
                return string.Empty; //TODO
            }
        }
    }
}
