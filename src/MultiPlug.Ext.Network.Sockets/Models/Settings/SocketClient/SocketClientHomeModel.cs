
namespace MultiPlug.Ext.Network.Sockets.Models.Settings.SocketClient
{
    public class SocketClientHomeModel
    {
        public bool Connected { get; internal set; }
        public bool ConnectionInError { get; internal set; }
        public string Guid { get; internal set; }
        public int LoggingLevel { get; internal set; }
        public string TraceLog { get; internal set; }
    }
}
