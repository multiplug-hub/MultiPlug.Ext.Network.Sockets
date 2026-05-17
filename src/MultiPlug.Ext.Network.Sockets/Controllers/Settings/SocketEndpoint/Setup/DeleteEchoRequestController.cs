using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;

namespace MultiPlug.Ext.Network.Sockets.Controllers.Settings.SocketEndpoint.Setup
{
    [Route("socketendpoint/setup/deleteechorequest")]
    public class DeleteEchoRequestController : SettingsApp
    {
        public Response Post(string id, string pingid)
        {
            if (id != null && pingid != null)
            {
                Core.Instance.RemoveSocketEndpointEchoRequest(id, pingid);
            }

            return new Response
            {
                StatusCode = System.Net.HttpStatusCode.OK
            };
        }
    }
}
