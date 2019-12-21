using MessagesLibrary;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        // Calculate FPS
        private volatile Int32 FramesPerSec = 0;

        // ZeroMQ message subscription
        ReadMessages Reader { get; set; }      

        public MainWindow()
        {
            InitializeComponent();

            ReadMessages();

            CalculateFPS();
        }

        private void ReadMessages()
        {
            Reader = new ReadMessages();
            Reader.MessageCallback = NewMessageCallback;
            Reader.Start();
        }

        // Calculate FPS
        void CalculateFPS()
        {
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

        // Clean up 
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Reader.Stop();
        }

        public void NewMessageCallback(Message msg)
        {
            switch(msg.GetMessageType())
            {
                case Message.MessageType.OpenCVMatFrame:
                    {
                        var image = LoadImage(msg);
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            if (image != null)
                            {
                                VideoImg.Source = image;
                            }
                        }));
                        break;
                    }
                default:
                    {
                        Debug.Assert(false, "Unhandled message type.");
                        break;
                    }
            }
        }

        private BitmapSource LoadImage(Message msg)
        {
            try
            {
                int fps = (int)msg.HeaderMap["fps"];
                int width = (int)msg.HeaderMap["width"];
                int height = (int)msg.HeaderMap["height"];
                int frameStep = (int)msg.HeaderMap["step"];
                byte[] imgBuffer = msg.GetData();

                var image = BitmapImage.Create(width, height, 96, 96, System.Windows.Media.PixelFormats.Bgr24, System.Windows.Media.Imaging.BitmapPalettes.WebPalette, imgBuffer, frameStep);
                return image;
            }
            catch(Exception ex)
            {
                Debug.Assert(false, $"LoadImage failed {ex.Message}.");
                return null;
            }
        }     
    }
}
