using System.Linq;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;

namespace MultiPlug.Ext.Network.Sockets.Controllers.Settings.SocketClient.Home
{
    [Route("socketclient/terminal")]
    public class SocketClientTerminalController : SettingsApp
    {
        public Response Post(string id, string send)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var SocketClient = Core.Instance.SocketClients.FirstOrDefault(Client => Client.Guid == id);

                if (SocketClient == null)
                {
                    return new Response
                    {
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }

                if( SocketClient.TerminalSend(send) )
                {
                    return new Response
                    {
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    return new Response
                    {
                        StatusCode = System.Net.HttpStatusCode.InternalServerError
                    };
                }
            }

            return new Response
            {
                StatusCode = System.Net.HttpStatusCode.NotFound
            };
        }
    }
}
