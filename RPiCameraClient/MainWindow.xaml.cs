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
using System.Windows.Media.Imaging;

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
                    IntPtr pBuffer = IntPtr.Zero;
                    ImportSubscriber.GetCurrentImage(ref pBuffer, ref size);
                    if (size > 0)
                    {
                        byte[] imgBuffer = new byte[size];
                        Marshal.Copy(pBuffer, imgBuffer, 0, imgBuffer.Length);

                        Application.Current.Dispatcher.Invoke(new Action(() => 
                        { 
                            VideoImg.Source = LoadImage(new MemoryStream(imgBuffer)); 
                        }));                        
                    }
                    // int size = imageBuffer.Length;
                } while (WaitEvent.WaitOne(100) == false);
            });
        }

        private BitmapImage LoadImage(Stream stream)
        {
            // assumes that the streams position is at the beginning
            // for example if you use a memory stream you might need to point it to 0 first
            var image = new BitmapImage();

            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();

            image.Freeze();
            return image;
        }
    }
}
