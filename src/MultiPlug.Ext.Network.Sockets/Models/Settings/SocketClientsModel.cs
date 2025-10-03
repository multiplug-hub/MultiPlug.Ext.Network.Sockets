using MultiPlug.Ext.Network.Sockets.Components.SocketClient;

namespace MultiPlug.Ext.Network.Sockets.Models.Settings
{
    public class SocketClientsModel
    {
        public string SocketClientCount { get; internal set; }
        public SocketClientComponent[] SocketClients { get; internal set; }
    }
}
