using System.Text;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MultiPlug.Base;
using MultiPlug.Base.Exchange;
using MultiPlug.Base.Exchange.API;
using System.Net.Sockets;

namespace MultiPlug.Ext.Network.Sockets.Models.Components
{
    public class SocketClientProperties : MultiPlugBase
    {
        public string Guid { get { return ReadEvent.Guid; } }

        [DataMember]
        public Event ReadEvent { get; set; }
        [DataMember]
        public List<Subscription> WriteSubscriptions { get; set; } = new List<Subscription>();
        [DataMember]
        public string HostName { get; set; } = string.Empty;
        [DataMember]
        public int Port { get; set; } = 0;
        [DataMember]
        public string EventKey { get; set; } = "value";
        [DataMember]
        public string SubscriptionKey { get; set; } = "value";

        //public string[] IPAddressList
        //{
        //    get
        //    {
        //        IPHostEntry ipHost = Dns.GetHostEntry("");
        //        return ipHost.AddressList.Select(a => a.ToString()).ToArray();
        //    }
        //}
        public string TraceLog
        {
            get
            {
                return (m_LoggingService == null)? string.Empty : string.Join(System.Environment.NewLine, m_LoggingService.Read());
            }
        }

        internal ILoggingService m_LoggingService;

        [DataMember]
        public int LoggingLevel { get; set; }
    }
}
