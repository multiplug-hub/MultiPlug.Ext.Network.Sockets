using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;

namespace MultiPlug.Ext.Network.Sockets.Controllers.Settings.SocketClient.Setup
{
    [Route("socketclient/setup/deleteechorequest")]
    public class DeleteEchoRequestController : SettingsApp
    {
        public Response Post(string id, string pingid)
        {
            if (id != null && pingid != null)
            {
                Core.Instance.RemoveSocketClientEchoRequest(id, pingid);
            }

            return new Response
            {
                StatusCode = System.Net.HttpStatusCode.OK
            };
        }
    }
}
