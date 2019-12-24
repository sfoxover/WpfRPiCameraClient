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
            set 
            { 
                if(SetProperty(ref _faceAiOff, value) && _faceAiOff)
                    UpdateFaceAIDetection("Off"); 
            }
        }

        private bool _faceAiOpenCV = false;
        public bool FaceAiOpenCV
        {
            get { return _faceAiOpenCV; }
            set 
            { 
                if(SetProperty(ref _faceAiOpenCV, value) && _faceAiOpenCV)
                    UpdateFaceAIDetection("OpenCV"); 
            }
        }

        private bool _faceAiDnn = false;
        public bool FaceAiDnn
        {
            get { return _faceAiDnn; }
            set 
            { 
                if(SetProperty(ref _faceAiDnn, value) && _faceAiDnn)
                    UpdateFaceAIDetection("Dnn"); 
            }
        }

        private bool _faceAiMod = false;
        public bool FaceAiMod
        {
            get { return _faceAiMod; }
            set 
            { 
                if(SetProperty(ref _faceAiMod, value) && _faceAiMod)
                    UpdateFaceAIDetection("Mod"); 
            }
        }

        private bool _faceAiHog = false;
        public bool FaceAiHog
        {
            get { return _faceAiHog; }
            set 
            { 
                if(SetProperty(ref _faceAiHog, value) && _faceAiHog)
                    UpdateFaceAIDetection("Hog"); 
            }
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
