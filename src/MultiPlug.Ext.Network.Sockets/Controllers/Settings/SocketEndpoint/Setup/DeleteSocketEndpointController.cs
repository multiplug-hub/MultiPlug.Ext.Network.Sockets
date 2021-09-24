using System.Linq;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.Network.Sockets.Components.SocketEndpoint;

namespace MultiPlug.Ext.Network.Sockets.Controllers.Settings.SocketEndpoint.Setup
{
    [Route("socketendpoint/delete")]
    public class DeleteSocketEndpointController : SettingsApp
    {
        public Response Post( string id )
        {
            if (id != null)
            {
                SocketEndpointComponent SocketEndpoint = Core.Instance.SocketEndpoints.FirstOrDefault(t => t.Guid == id);

                if (SocketEndpoint != null)
                {
                    Core.Instance.Remove(new[] { SocketEndpoint });
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
