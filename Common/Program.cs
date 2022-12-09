using System.Net.Sockets;
using System.Net;

namespace Common
{

    public class COMMON
    {

        public static Socket _listener = new Socket(
                                AddressFamily.InterNetwork,
                                SocketType.Dgram,
                                ProtocolType.Udp
                                );
        public static IPAddress _ip = IPAddress.Loopback;
        public static IPEndPoint _listenEP = new IPEndPoint(_ip, 27001);
        public static byte[] _buffer = new byte[ushort.MaxValue];

        public static EndPoint _endPoint = new IPEndPoint(IPAddress.Any, 0);

        public static void Init()
        {
            _listener.Bind(_listenEP);
        }

        static void Main(string[] args)
        {
        }
    }
}