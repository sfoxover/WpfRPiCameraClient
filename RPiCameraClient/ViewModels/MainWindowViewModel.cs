using MaterialDesignThemes.Wpf;
using MessagesLibrary;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RPiCameraClient.ViewModels
{
    internal class MainWindowViewModel : ViewModelHelper
    {
        // Popup message window
        private Snackbar MainWndSnackbar = null;

        // Face detection AI values
        private bool _faceAiOff = true;
        public bool FaceAiOff
        {
            get { return _faceAiOff; }
            set { SetProperty(ref _faceAiOff, value); UpdateFaceAIDetection("Off"); }
        }

        private bool _faceAiOpenCV = false;
        public bool FaceAiOpenCV
        {
            get { return _faceAiOpenCV; }
            set { SetProperty(ref _faceAiOpenCV, value); UpdateFaceAIDetection("OpenCV"); }
        }

        private bool _faceAiDnn = false;
        public bool FaceAiDnn
        {
            get { return _faceAiDnn; }
            set { SetProperty(ref _faceAiDnn, value); UpdateFaceAIDetection("Dnn"); }
        }

        private bool _faceAiMod = false;
        public bool FaceAiMod
        {
            get { return _faceAiMod; }
            set { SetProperty(ref _faceAiMod, value); UpdateFaceAIDetection("Mod"); }
        }

        private bool _faceAiHog = false;
        public bool FaceAiHog
        {
            get { return _faceAiHog; }
            set { SetProperty(ref _faceAiHog, value); UpdateFaceAIDetection("Hog"); }
        }

        public MainWindowViewModel(Snackbar snackbar)
        {
            MainWndSnackbar = snackbar;
        }

        void UpdateFaceAIDetection(string method)
        {
            Task.Run(() =>
            {
                // Create a message payload
                var items = new Dictionary<string, object>();
                items["Command"] = "SetAIMethod";
                items["Value"] = method;
                var msg = Message.CreateMessageFromJson("ServerCommand", Message.MessageType.ServerCommand, items);

                // Send command
                var cmd = new SendCommand();
                bool bOK = cmd.SendCommandMessage(msg, out string error);
            });
        }
    }
}
