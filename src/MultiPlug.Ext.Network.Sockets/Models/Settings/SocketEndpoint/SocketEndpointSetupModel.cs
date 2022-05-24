
namespace MultiPlug.Ext.Network.Sockets.Models.Settings.SocketEndpoint
{
    public class SocketEndpointSetupModel
    {
        public string Guid { get; internal set; }
        public int Backlog { get; internal set; }
        public int NICIndex { get; internal set; }
        public string[] IPAddressList { get; internal set; }
        public int Port { get; internal set; }
        public string ReadEventId { get; internal set; }
        public string ReadEventDescription { get; internal set; }
        public string ReadEventSubject { get; internal set; }
        public string[] WriteSubscriptionGuid { get; internal set; }
        public string[] WriteSubscriptionId { get; internal set; }
        public bool[] WriteSubscriptionConnected { get; internal set; }
        public bool SubscriptionsControlConnect { get; internal set; }
        public string[] AllowedList { get; set; }
    }
}
