using MessagesLibrary;
using RPiCameraClient.ViewModels;
using System;
using System.Collections.Generic;
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

        private MainWindowViewModel ViewModel
        {
            get { return DataContext as MainWindowViewModel; }
        }
        
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel(MainSnackbar);

            Initialize();
        }

        void Initialize()
        {
            bool bOK = Settings.Instance.Initialize(Helpers.AppendToRunPath("message_developer.json"), out string error);
            Debug.Assert(bOK, error);
            ReadMessages();
            CalculateFPS();
        }

        private void ReadMessages()
        {
            Task.Run(() =>
            {
                List<string> topics = new List<string>();
                topics.Add(Settings.Instance.VideoCamTopic);
                topics.Add(Settings.Instance.VideoSampleTopic);
                topics.Add(Settings.Instance.FaceDetectTopic);
                topics.Add(Settings.Instance.ProfilingTopic);
                Reader = new ReadMessages(topics);
                Reader.MessageCallback = NewMessageCallback;
                Reader.Start();
            });
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
            if (Reader != null)
            {
                Reader.Stop();
            }
        }

        // Call back with new message object
        public void NewMessageCallback(Message msg)
        {
            try
            {
                switch (msg.GetMessageType())
                {
                    case Message.MessageType.OpenCVMatFrame:
                        {
                            var image = LoadImage(msg);
                            image.Freeze();
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                if (image != null)
                                {
                                    FramesPerSec++;
                                    VideoImg.Source = image;
                                }
                            }));
                            break;
                        }
                    case Message.MessageType.FaceDetection:
                        {
                            var image = LoadImage(msg);
                            image.Freeze();
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                if (image != null)
                                {
                                    FaceDetectionImg.Source = image;
                                }
                            }));
                            break;
                        }
                    case Message.MessageType.ProfilingData:
                        {
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                msg.GetHeaderMapValue("AiImagesPerSec", out object fps);
                                LabelFaceDetectFPS.Content = $"Face detection frames per second: {Convert.ToInt32(fps)}";

                                msg.GetHeaderMapValue("CpuUsage", out object cpu);
                                LabelCpuUsage.Content = $"Service CPU usage: {Convert.ToInt32(cpu)}%";
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
            catch(Exception ex)
            {
                Debug.WriteLine($"NewMessageCallback exception {ex.Message}");
            }
        }

        // Convert message buffer cv::Mat image to Bgr24 bitmap source
        private BitmapSource LoadImage(Message msg)
        {
            try
            {
                int width = Convert.ToInt32(msg.HeaderMap["width"]);
                int height = Convert.ToInt32(msg.HeaderMap["height"]);
                int frameStep = Convert.ToInt32(msg.HeaderMap["step"]);
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
