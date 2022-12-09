using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ResizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn)
            {
                switch (btn.Content.ToString())
                {
                    case "🗕":
                        WindowState = WindowState.Minimized;
                        break;
                    case "X":
                        this.Close();
                        break;
                    default:
                        break;
                }
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void GetScreenshot_Click(object sender, RoutedEventArgs e)
        {
            var client = new Socket(
                                    AddressFamily.InterNetwork,
                                    SocketType.Dgram,
                                    ProtocolType.Udp
                                    );

            IPAddress.TryParse("127.0.0.1", out var ip);
            byte[] buffer = new byte[ushort.MaxValue];
            EndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 27001);

            client.SendTo(Encoding.Default.GetBytes("get"), SocketFlags.None, endPoint);


            var len = client.ReceiveFrom(buffer, SocketFlags.None, ref endPoint);
            string response = Encoding.Default.GetString(buffer, 0, len);

            // Check response
            if (response.ToLower() != "start")
                return;


            // Get number of parts
            len = client.ReceiveFrom(buffer, SocketFlags.None, ref endPoint);
            int numberParts = int.Parse(Encoding.Default.GetString(buffer, 0, len));
            client.SendTo(Encoding.Default.GetBytes("received"), SocketFlags.None, endPoint);

            // Get lenght of array
            len = client.ReceiveFrom(buffer, SocketFlags.None, ref endPoint);
            int lenght = int.Parse(Encoding.Default.GetString(buffer, 0, len));
            client.SendTo(Encoding.Default.GetBytes("received"), SocketFlags.None, endPoint);
            byte[] responseImage = new byte[lenght];

            // Recive screenshot
            int received = 0;
            for (int i = 0; i < numberParts; i++)
            {

                len = client.ReceiveFrom(buffer, SocketFlags.None, ref endPoint);

                // Add data to responseImage
                for (int j = received,k = 0; k < len; j++,k++)
                {
                    responseImage[j] = buffer[k];
                }

                client.SendTo(Encoding.Default.GetBytes("received"), SocketFlags.None, endPoint);

                received += len;
            }

            // Convert Bytes

            // Assign image to WPF Image
            var image = ByteToImage(responseImage);
            ImageArea.Source = image;

        }

        public static ImageSource ByteToImage(byte[] imageData)
        {
            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(imageData);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();



            ImageSource imgSrc = biImg as ImageSource;



            return imgSrc;
        }

        public BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }
    }
}
