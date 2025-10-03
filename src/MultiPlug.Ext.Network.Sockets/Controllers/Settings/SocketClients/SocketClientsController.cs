using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;

namespace MultiPlug.Ext.Network.Sockets.Controllers.Settings.SocketClients
{
    [Route("socketclients")]
    public class SocketClientsController : SettingsApp
    {
        public Response Get()
        {
            return new Response
            {
                Model = new Models.Settings.SocketClientsModel
                {
                    SocketClientCount = Core.Instance.SocketClients.Length.ToString(),
                    SocketClients = Core.Instance.SocketClients,
                },
                Template = Templates.SettingsSocketClients
            };
        }
    }
}