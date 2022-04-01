
namespace MultiPlug.Ext.Network.Sockets.Models.Settings.SocketClient
{
    public class SocketClientSetupModel
    {
        public string Guid { get; internal set; }
        public string HostName { get; internal set; }
        public int Port { get; internal set; }
        public string ReadEventDescription { get; internal set; }
        public string ReadEventId { get; internal set; }
        public string ReadEventSubject { get; internal set; }
        public bool[] WriteSubscriptionConnected { get; internal set; }
        public string[] WriteSubscriptionGuid { get; internal set; }
        public string[] WriteSubscriptionId { get; internal set; }
    }
}
