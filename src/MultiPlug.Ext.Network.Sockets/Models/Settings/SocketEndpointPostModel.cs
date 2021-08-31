
namespace MultiPlug.Ext.Network.Sockets.Models.Settings
{
    public class SocketEndpointPostModel
    {
        public string Guid { get; set; }
        public string EventId { get; set; }
        public string EventDescription { get; set; }
        public string[] SubscriptionGuid { get; set; }
        public string[] SubscriptionId { get; set; }
        public string EventKey { get; set; }
        public string SubscriptionKey { get; set; }
        public string IPAddress { get; set; }
        public string Port { get; set; }
        public string Backlog { get; set; }
        public int LoggingLevel { get; set; }
    }
}
