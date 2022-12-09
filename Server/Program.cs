using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            var listener = new Socket(
                                     AddressFamily.InterNetwork,
                                     SocketType.Dgram,
                                     ProtocolType.Udp
                                     );
            IPAddress ip = IPAddress.Loopback;
            var listenEP = new IPEndPoint(ip, 27001);
            listener.Bind(listenEP);
            var buffer = new byte[ushort.MaxValue];
            EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                var len = listener.ReceiveFrom(buffer, ref endPoint);
                string request = Encoding.Default.GetString(buffer, 0, len);

                // Check request
                if (request.ToLower() != "get")
                    continue;

                // Send response for begin of parts
                listener.SendTo(Encoding.Default.GetBytes("start"), SocketFlags.None, endPoint);

                // Get Screenshot
                Bitmap memoryImage;
                memoryImage = new Bitmap(1600, 1080);
                Size s = new Size(memoryImage.Width, memoryImage.Height);
                Graphics memoryGraphics = Graphics.FromImage(memoryImage);
                memoryGraphics.CopyFromScreen(0, 0, 0, 0, s);

                // Convert Bytes
                ImageConverter converter = new ImageConverter();
                var bytes = (byte[])converter.ConvertTo(memoryImage, typeof(byte[]));

                // Find & Sending number of part
                int numberOfParts = (bytes.Length / 40_000) + 1;
                listener.SendTo(Encoding.Default.GetBytes(numberOfParts.ToString()), SocketFlags.None, endPoint);
                listener.ReceiveFrom(buffer, ref endPoint);

                // Find & Sending number of part
                listener.SendTo(Encoding.Default.GetBytes(bytes.Length.ToString()), SocketFlags.None, endPoint);
                listener.ReceiveFrom(buffer, ref endPoint);


                // Divide into parts and send
                int count = 0;
                int sended = 0;
                for (int i = 0; i < numberOfParts; i++)
                {

                    sended += listener.SendTo(bytes.Skip(count).Take(40000).ToArray(), SocketFlags.None, endPoint);
                    listener.ReceiveFrom(buffer, ref endPoint);
                    count += 40000;

                }

            }

        }

        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
    }
}