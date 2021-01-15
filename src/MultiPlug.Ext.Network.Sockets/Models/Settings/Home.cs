using MultiPlug.Base;
using MultiPlug.Ext.Network.Sockets.Models.Components;

namespace MultiPlug.Ext.Network.Sockets.Models.Settings
{
    public class Home : MultiPlugBase
    {
        public string SocketEndpointCount { get; set; }
        public string SocketClientCount { get; set; }
        public SocketClientProperties[] SocketClients { get; set; }
        public SocketEndpointProperties[] SocketEndpoints { get; set; }
    }
}
