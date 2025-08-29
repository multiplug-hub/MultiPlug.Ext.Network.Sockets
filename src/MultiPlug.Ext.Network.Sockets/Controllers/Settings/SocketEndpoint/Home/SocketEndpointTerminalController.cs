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
                var SocketEndpoint = Core.Instance.SocketEndpoints.FirstOrDefault(Client => Client.Guid == id);

                if (SocketEndpoint == null)
                {
                    return new Response
                    {
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }

                SocketEndpoint.TerminalSend(send);

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
