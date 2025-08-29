
using MultiPlug.Ext.Network.Sockets.Models.Components;

namespace MultiPlug.Ext.Network.Sockets.Models.Settings.SocketEndpoint
{
    public class SocketEndpointHomeModel
    {
        public string Guid { get; set; }
        public ConnectedClient[] ConnectedClients { get; internal set; }
        public int LoggingLevel { get; internal set; }
        public string TraceLog { get; internal set; }
        public bool LoggingShowControlCharacters { get; internal set; }
    }
}
