using System.Linq;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.Network.Sockets.Components.SocketClient;

namespace MultiPlug.Ext.Network.Sockets.Controllers.Settings.SocketClient
{
    [Route("socketclient/delete")]
    public class DeleteSocketClientController : SettingsApp
    {
        public Response Post(string id)
        {
            if( id != null)
            {
                SocketClientComponent SocketClient = Core.Instance.SocketClients.FirstOrDefault(t => t.Guid == id);

                if( SocketClient != null)
                {
                    Core.Instance.Remove(new[] { SocketClient } );
                }
            }

            return new Response
            {
                StatusCode = System.Net.HttpStatusCode.Redirect,
                Location = Context.Referrer
            };
        }
    }
}
