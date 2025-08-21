
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
        public string[] WriteSubscriptionGuids { get; internal set; }
        public string[] WriteSubscriptionIds { get; internal set; }
        public bool[] WriteSubscriptionConnecteds { get; internal set; }
        public bool SubscriptionsControlConnect { get; internal set; }
        public string[] AllowedList { get; set; }
        public string[] WriteSubscriptionWritePrefixs { get; internal set; }
        public string[] WriteSubscriptionWriteSeparators { get; internal set; }
        public string[] WriteSubscriptionWriteSuffixs { get; internal set; }
        public bool[] WriteSubscriptionIsHexs { get; internal set; }
        public bool[] WriteSubscriptionIgnoreDatas { get; internal set; }
    }
}
