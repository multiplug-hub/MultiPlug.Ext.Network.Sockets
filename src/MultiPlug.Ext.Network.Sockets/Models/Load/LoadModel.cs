using System.Runtime.Serialization;
using MultiPlug.Base;
using MultiPlug.Ext.Network.Sockets.Models.Components;

namespace MultiPlug.Ext.Network.Sockets.Models.Load
{
    public class LoadModel : MultiPlugBase
    {
        [DataMember]
        public SocketEndpointProperties[] SocketEndpoints { get; set; }
        [DataMember]
        public SocketClientProperties[] SocketClients { get; set; }
    }
}