
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
        public bool[] WriteSubscriptionConnecteds { get; internal set; }
        public string[] WriteSubscriptionGuids { get; internal set; }
        public string[] WriteSubscriptionIds { get; internal set; }
        public bool SubscriptionsControlConnect { get; internal set; }
        public string[] WriteSubscriptionWritePrefixs { get; internal set; }
        public string[] WriteSubscriptionWriteSeparators { get; internal set; }
        public string[] WriteSubscriptionWriteSuffixs { get; internal set; }
        public bool[] WriteSubscriptionIsHexs { get; internal set; }
        public bool[] WriteSubscriptionIgnoreDatas { get; internal set; }
        public bool ReadTrim { get; internal set; }
        public string ReadPrefix { get; internal set; }
        public string ReadAppend { get; internal set; }
    }
}
