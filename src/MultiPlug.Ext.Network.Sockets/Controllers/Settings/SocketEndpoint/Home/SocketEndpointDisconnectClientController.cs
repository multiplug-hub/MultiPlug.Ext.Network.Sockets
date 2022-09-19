using System;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.Network.Sockets.Models.Settings.SocketEndpoint;

namespace MultiPlug.Ext.Network.Sockets.Controllers.Settings.SocketEndpoint.Home
{
    [Route("socketendpoint/disconnect/")]
    public class SocketEndpointDisconnectClientController : SettingsApp
    {
        public Response Post(SocketEndpointDisconnectClientModel theModel)
        {
            if (theModel != null && !string.IsNullOrEmpty(theModel.Guid) && !string.IsNullOrEmpty(theModel.ClientGuid))
            {
                Core.Instance.SocketEndpointDisconnectClient(theModel.Guid, theModel.ClientGuid);

                return new Response
                {
                    StatusCode = System.Net.HttpStatusCode.Moved,
                    Location = new Uri(Context.Referrer, Context.Referrer.AbsolutePath + "?id=" + theModel.Guid)
                };
            }

            return new Response
            {
                StatusCode = System.Net.HttpStatusCode.Moved,
                Location = Context.Referrer
            };
        }
    }
}
