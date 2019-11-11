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
            
            bool bOK = ImportSubscriber.StartSubscription("video1");
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
                    int size = 0;
                    IntPtr pBuffer = IntPtr.Zero;
                    ImportSubscriber.GetCurrentImage(ref pBuffer, ref size);
                    if (size > 0)
                    {
                        FramesPerSec++;
                        byte[] imgBuffer = new byte[size];
                        Marshal.Copy(pBuffer, imgBuffer, 0, imgBuffer.Length);

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

        private void ReadSensors()
        {
            Task.Run(() =>
            {
                do
                {
                    int size = 0;
                    IntPtr pBuffer = IntPtr.Zero;
                    ImportSubscriber.GetCurrentSensorData(ref pBuffer, ref size);
                    if (size > 0)
                    {
                        byte[] dataBuffer = new byte[size];
                        Marshal.Copy(pBuffer, dataBuffer, 0, dataBuffer.Length);

                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            LabelSensors.Content = System.Text.Encoding.UTF8.GetString(dataBuffer);
                        }));
                    }
                } while (WaitEvent.WaitOne(10) == false);
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
