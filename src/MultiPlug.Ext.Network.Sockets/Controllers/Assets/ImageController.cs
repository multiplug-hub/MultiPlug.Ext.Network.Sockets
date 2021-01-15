using System.Drawing;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.Network.Sockets.Properties;

namespace MultiPlug.Ext.Network.Sockets.Controllers.Assets
{
    [Route("images/*")]
    public class ImageController : AssetsEndpoint
    {
        public Response Get(string theName)
        {
            ImageConverter converter = new ImageConverter();
            return new Response { RawBytes = (byte[])converter.ConvertTo(Resources.NetworksIcon, typeof(byte[])), MediaType = "image/png" };
        }
    }
}
