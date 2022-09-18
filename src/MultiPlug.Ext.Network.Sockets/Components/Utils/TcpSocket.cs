
using System.Net.Sockets;

// https://stackoverflow.com/questions/3457521/how-to-check-if-object-has-been-disposed-in-c-sharp
namespace MultiPlug.Ext.Network.Sockets.Components.Utils
{
    public class TcpSocket : Socket
    {
        public TcpSocket(SocketInformation socketInformation) : base(socketInformation)
        {
        }

        public TcpSocket(SocketType socketType, ProtocolType protocolType) : base(socketType, protocolType)
        {
        }

        public TcpSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) : base(addressFamily, socketType, protocolType)
        {
        }

        public bool IsDisposed { get; set; }
        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}
