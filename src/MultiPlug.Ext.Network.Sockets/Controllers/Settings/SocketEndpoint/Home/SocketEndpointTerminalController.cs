using System.Linq;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;

namespace MultiPlug.Ext.Network.Sockets.Controllers.Settings.SocketEndpoint.Home
{
    [Route("socketendpoint/terminal")]
    public class SocketEndpointTerminalController : SettingsApp
    {
        public Response Post(string id, string send)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var SocketClient = Core.Instance.SocketEndpoints.FirstOrDefault(Client => Client.Guid == id);

                if (SocketClient == null)
                {
                    return new Response
                    {
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }

                SocketClient.TerminalSend(send);

                return new Response
                {
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }

            return new Response
            {
                StatusCode = System.Net.HttpStatusCode.NotFound
            };
        }
    }
}
