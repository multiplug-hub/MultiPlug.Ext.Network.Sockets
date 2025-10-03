using System.Text;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.Network.Sockets.Properties;

namespace MultiPlug.Ext.Network.Sockets.Controllers.Assets
{
    [Route("js/*")]
    public class JavaScriptsController : AssetsEndpoint
    {
        public Response Get(string theName)
        {
            string Result = string.Empty;

            switch (theName)
            {
                case "jquery.dirty.min.js":
                    Result = Resources.jquery_dirty_min_js;
                    break;
                default:
                    break;
            }

            if (string.IsNullOrEmpty(Result))
            {
                return new Response { StatusCode = System.Net.HttpStatusCode.NotFound };
            }
            else
            {
                return new Response { MediaType = "text/javascript", RawBytes = Encoding.ASCII.GetBytes(Result) };
            }
        }
    }
}