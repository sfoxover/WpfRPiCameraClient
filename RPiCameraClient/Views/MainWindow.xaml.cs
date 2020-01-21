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

        // Calculate bandwidth
        private volatile Int32 BytesPerSec = 0;

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
            CalculateClientProfiling();
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

        // Calculate client FPS and bandwidth
        void CalculateClientProfiling()
        {
            // Calculate FPS 
            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += (s, args) =>
            {
                timer.IsEnabled = true;
                LabelFPS.Content = $"Frames per second: {FramesPerSec}";
                LabelClientBandwidth.Content = $"Client bandwidth: {Helpers.FormatBandwidth(BytesPerSec)}";
                FramesPerSec = 0;
                BytesPerSec = 0;
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
                // Used to calculate bandwidth used
                BytesPerSec += msg.GetDataSize();

                Int32 type = msg.GetMessageType();

                if (((Message.MessageType)type & Message.MessageType.Video) == Message.MessageType.Video)
                {
                    BitmapSource image = null;
                    if (((Message.MessageType)type & Message.MessageType.OpenCVMatFrame) == Message.MessageType.OpenCVMatFrame)
                    {
                        image = LoadImage(msg);
                    }
                    else if (((Message.MessageType)type & Message.MessageType.JpegFrame) == Message.MessageType.JpegFrame)
                    {
                        image = LoadImage(new MemoryStream(msg.GetData()));

                    }
                    if (image != null)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            if (((Message.MessageType)type & Message.MessageType.FaceDetection) == Message.MessageType.FaceDetection)
                            {
                                FaceDetectionImg.Source = image;
                            }
                            else
                            {
                                FramesPerSec++;
                                VideoImg.Source = image;
                            }
                        }));
                    }
                }
                else if (((Message.MessageType)type & Message.MessageType.ProfilingData) == Message.MessageType.ProfilingData)
                { 
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        msg.GetHeaderMapValue("AiImagesPerSec", out object fps);
                        LabelFaceDetectFPS.Content = $"Face detection frames per second: {Convert.ToInt32(fps)}";

                        msg.GetHeaderMapValue("CpuUsage", out object cpuUsage);
                        LabelCpuUsage.Content = $"Service CPU usage: {Convert.ToInt32(cpuUsage)}%";

                        msg.GetHeaderMapValue("CpuTempature", out object cpuTemp);
                        LabelCpuTemperature.Content = $"Service CPU temperature: {Convert.ToInt32(cpuTemp)} Celsius";
                    }));
                }
                else
                {
                    Debug.Assert(false, "Unhandled message type.");
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
                
                image.Freeze();
                return image;
            }
            catch(Exception ex)
            {
                Debug.Assert(false, $"LoadImage failed {ex.Message}.");
                return null;
            }
        }

        private BitmapImage LoadImage(Stream stream)
        {
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
