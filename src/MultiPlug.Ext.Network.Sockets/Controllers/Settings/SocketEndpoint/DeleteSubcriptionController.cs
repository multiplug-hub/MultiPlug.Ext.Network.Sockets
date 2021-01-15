using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;

namespace MultiPlug.Ext.Network.Sockets.Controllers.Settings.SocketEndpoint
{
    [Route("socketendpoint/deletesubscription")]
    public class DeleteSubcriptionController : SettingsApp
    {
        public Response Post(string id, string subid)
        {
            if (id != null && subid != null)
            {
                Core.Instance.RemoveSocketEndpointSubcription(id, subid);
            }

            return new Response
            {
                StatusCode = System.Net.HttpStatusCode.OK
            };
        }
    }
}
