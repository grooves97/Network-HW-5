using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerScreenshot
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, 23456));
                socket.Listen(100);
                while (true)
                {
                    using (var client = socket.Accept())
                    {
                        var bounds = Screen.PrimaryScreen.Bounds;
                        var bitmap = new Bitmap(bounds.Width, bounds.Height);
                        try
                        {
                            while (true)
                            {
                                using (var graphics = Graphics.FromImage(bitmap))
                                {
                                    graphics.CopyFromScreen(bounds.X, 0, bounds.Y, 0, bounds.Size);
                                }
                                byte[] imageData;
                                using (var stream = new MemoryStream())
                                {
                                    bitmap.Save(stream, ImageFormat.Png);
                                    imageData = stream.ToArray();
                                }
                                var lengthData = BitConverter.GetBytes(imageData.Length);
                                if (client.Send(lengthData) < lengthData.Length) break;
                                if (client.Send(imageData) < imageData.Length) break;
                                Thread.Sleep(1000);
                            }
                        }
                        catch
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}
