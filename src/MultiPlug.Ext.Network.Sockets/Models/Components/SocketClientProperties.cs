using System.Runtime.Serialization;
using MultiPlug.Base;
using MultiPlug.Base.Exchange;
using MultiPlug.Base.Exchange.API;

namespace MultiPlug.Ext.Network.Sockets.Models.Components
{
    public class SocketClientProperties : MultiPlugBase
    {
        [DataMember]
        public string Guid { get; set; }

        [DataMember]
        public Event ReadEvent { get; set; }
        [DataMember]
        public Subscription[] WriteSubscriptions { get; set; }
        [DataMember]
        public string HostName { get; set; }
        [DataMember]
        public int Port { get; set; } = -1;
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
