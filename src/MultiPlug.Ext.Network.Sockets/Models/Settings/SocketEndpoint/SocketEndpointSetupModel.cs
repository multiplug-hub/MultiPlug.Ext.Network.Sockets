
namespace MultiPlug.Ext.Network.Sockets.Models.Settings.SocketEndpoint
{
    public class SocketEndpointSetupModel
    {
        public string Guid { get; set; }
        public int Backlog { get; internal set; }
        public int NICIndex { get; set; }

        public string[] IPAddressList { get; set; }
        public int Port { get; set; }

        public string ReadEventId { get; set; }
        public string ReadEventDescription { get; set; }
        public string ReadEventSubject { get; set; }

        public string[] WriteSubscriptionGuid { get; set; }
        public string[] WriteSubscriptionId { get; set; }
        public string[] WriteSubscriptionIndex { get; set; }
        public bool[] WriteSubscriptionConnected { get; set; }
    }
}
