using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;

namespace MultiPlug.Ext.Network.Sockets.Controllers.Settings.Home
{
    [Route("")]
    public class HomeController : SettingsApp
    {
        public Response Get()
        {
            return new Response
            {
                Model = new Models.Settings.Home
                {
                    SocketClientCount = Core.Instance.SocketClients.Length.ToString(),
                    SocketEndpointCount = Core.Instance.SocketEndpoints.Length.ToString()
                },
                Template = Templates.SettingsHome
            };
        }
    }
}
