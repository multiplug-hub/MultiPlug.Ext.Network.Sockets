using MultiPlug.Ext.Network.Sockets.Components.SocketEndpoint;

namespace MultiPlug.Ext.Network.Sockets.Models.Settings
{
    public class SocketEndpointsModel
    {
        public string SocketEndpointCount { get; internal set; }
        public SocketEndpointComponent[] SocketEndpoints { get; internal set; }
    }
}
