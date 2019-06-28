using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientScreenshot
{
    public partial class Client : Form
    {
        private Bitmap _buffer;
        public Client()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            ThreadPool.QueueUserWorkItem(GetSnapshots);
        }

        private void GetSnapshots(object state)
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(new IPEndPoint(IPAddress.Loopback, 23456));
                while (true)
                {
                    var lengthData = new byte[4];
                    var lengthBytesRead = 0;
                    while (lengthBytesRead < lengthData.Length)
                    {
                        var read = socket.Receive(lengthData, lengthBytesRead, lengthData.Length - lengthBytesRead, SocketFlags.None);
                        if (read == 0) return;
                        lengthBytesRead += read;
                    }
                    var length = BitConverter.ToInt32(lengthData, 0);
                    var imageData = new byte[length];
                    var imageBytesRead = 0;
                    while (imageBytesRead < imageData.Length)
                    {
                        var read = socket.Receive(imageData, imageBytesRead, imageData.Length - imageBytesRead, SocketFlags.None);
                        if (read == 0) return;
                        imageBytesRead += read;
                    }
                    using (var stream = new MemoryStream(imageData))
                    {
                        var bitmap = new Bitmap(stream);
                        Invoke(new ImageCompleteDelegate(ImageComplete), new object[] { bitmap });
                    }
                }
            }
        }

        private delegate void ImageCompleteDelegate(Bitmap bitmap);

        private void ImageComplete(Bitmap bitmap)
        {
            if (_buffer != null)
            {
                _buffer.Dispose();
            }
            _buffer = new Bitmap(bitmap);
            pictureBox1.Size = _buffer.Size;
            pictureBox1.Invalidate();
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (_buffer == null) return;
            e.Graphics.DrawImage(_buffer, 0, 0);
        }
    }
}
