using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Solomon.TypesExtensions
{
    public static class SocketExtensions
    {
        public static bool IsConnected(this Socket socket)
        {
            try
            {
                return !(socket.Available == 0 && socket.Poll(0, SelectMode.SelectRead));
            }
            catch (Exception) { return false; }
        }
    }
}
