using System.Runtime.Serialization;

namespace MultiPlug.Ext.Network.Sockets.Models.Components
{
    public class PingPong
    {
        public string Id { get; set; }
        [DataMember]
        public string Read { get; set; }
        public string ReadUnescaped { get; set; }
        [DataMember]
        public string Write { get; set; }
        public string WriteUnescaped { get; set; }
    }
}
