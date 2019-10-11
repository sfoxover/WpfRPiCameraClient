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
using System.Windows.Threading;

namespace RPiCameraClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static AutoResetEvent WaitEvent = new AutoResetEvent(false);

        // Calculate FPS
        private volatile Int32 FramesPerSec = 0;

        public MainWindow()
        {
            InitializeComponent();

            bool bOK = ImportSubscriber.StartSubscription();
            Debug.WriteLine($"StartSubscription {bOK}");

            ReadImages();
        }

        // Clean up 
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WaitEvent.Set();
            ImportSubscriber.StopSubscription();
        }

        private void ReadImages()
        {
            Task.Run(() =>
            {
                do
                {
                    byte[] imgBuffer = null;
                    int size = 0;
                    unsafe
                    {
                        ImportSubscriber.GetCurrentImage(out byte** pBuffer, ref size);
                        imgBuffer = new byte[size];
                        if (size > 0)
                        {
                            Marshal.Copy((IntPtr)pBuffer, imgBuffer, 0, imgBuffer.Length);
                        }
                    }
                    if (size > 0)
                    {
                        FramesPerSec++;
                        Application.Current.Dispatcher.Invoke(new Action(() => 
                        { 
                            VideoImg.Source = LoadImage(new MemoryStream(imgBuffer)); 
                        }));                        
                    }
                    // int size = imageBuffer.Length;
                } while (WaitEvent.WaitOne(10) == false);
            });

            // Calculate FPS 
            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += (s, args) =>
            {
                timer.IsEnabled = true;
                LabelFPS.Content = $"Frames per second: {FramesPerSec}";
                FramesPerSec = 0;
            };
            timer.Start();
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
