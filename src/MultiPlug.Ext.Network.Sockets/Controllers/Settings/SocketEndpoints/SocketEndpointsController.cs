using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;

namespace MultiPlug.Ext.Network.Sockets.Controllers.Settings.SocketEndpoints
{
    [Route("socketendpoints")]
    public class SocketEndpointsController : SettingsApp
    {
        public Response Get()
        {
            return new Response
            {
                Model = new Models.Settings.SocketEndpointsModel
                {
                    SocketEndpointCount = Core.Instance.SocketEndpoints.Length.ToString(),
                    SocketEndpoints = Core.Instance.SocketEndpoints
                },
                Template = Templates.SettingsSocketEndpoints
            };
        }
    }
}
