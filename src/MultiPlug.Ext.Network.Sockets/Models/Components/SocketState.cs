using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MultiPlug.Ext.Network.Sockets.Models.Components
{
    internal class SocketState
    {
        public SocketState()
        {
            Guid = System.Guid.NewGuid().ToString();
        }
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 1024;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
        public bool Errored { get; set; } = false;
        public string Address { get; internal set; }
        public string Guid { get; internal set; }
    }
}
