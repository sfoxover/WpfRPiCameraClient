using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RPiCameraClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static AutoResetEvent WaitEvent = new AutoResetEvent(false);

        public MainWindow()
        {
            InitializeComponent();

            bool bOK = ImportSubscriber.StartSubscription();
            Debug.WriteLine($"StartSubscription {bOK}");

            ReadImages();
        }

        private void ReadImages()
        {
            Task.Run(() =>
            {
                do
                {
                    int size = 0;
                    ImportSubscriber.GetCurrentImage(out byte[] buffer, ref size);
                    if (size > 0)
                    {
                        Image image = Image.FromStream(new MemoryStream(buffer));
                    }
                    // int size = imageBuffer.Length;
                } while (WaitEvent.WaitOne(100) == false);
            });
        }
    }
}
