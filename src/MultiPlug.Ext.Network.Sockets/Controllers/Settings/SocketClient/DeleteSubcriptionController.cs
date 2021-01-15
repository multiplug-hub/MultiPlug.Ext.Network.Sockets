using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;

namespace MultiPlug.Ext.Network.Sockets.Controllers.Settings.SocketClient
{
    [Route("socketclient/deletesubscription")]
    public class DeleteSubcriptionController : SettingsApp
    {
        public Response Post(string id, string subid)
        {
            if (id != null && subid != null)
            {
                Core.Instance.RemoveSocketClientSubcription(id, subid);
            }

            return new Response
            {
                StatusCode = System.Net.HttpStatusCode.OK
            };
        }
    }
}
