using System;
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
        private static AutoResetEvent WaitEvent = new AutoResetEvent(false);

        // Calculate FPS
        private volatile Int32 FramesPerSec = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        // Clean up 
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WaitEvent.Set();
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
