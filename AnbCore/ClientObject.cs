using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AnbCore
{
    class ClientObject
    {
        // Client socket.
        public Socket WorkSocket = null;
        // Size of receive buffer.
        public int BufferSize = 256;
        // Receive buffer.
        public byte[] Buffer = new byte[256];
        // Received data string.
        public StringBuilder Content = new StringBuilder();
    }
}
