using System.Net;
using System.Linq;

namespace MultiPlug.Ext.Network.Sockets.Components.Utils
{
    internal static class LocalIPAddressList
    {
        internal static string[] Get()
        {
            IPHostEntry ipHost = Dns.GetHostEntry("");
            return ipHost.AddressList.Select(a => a.ToString()).ToArray();
        }

        internal static int GetIndex( string theIPAdress )
        {
            string[] List = Get();

            for(var Index = 0; Index < List.Length; Index++)
            {
                if( theIPAdress == List[Index])
                {
                    return Index;
                }
            }

            return -1;
        }
    }
}
