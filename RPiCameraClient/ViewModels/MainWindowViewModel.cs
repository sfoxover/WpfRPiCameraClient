using MaterialDesignThemes.Wpf;
using MessagesLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace RPiCameraClient.ViewModels
{
    internal class MainWindowViewModel : ViewModelHelper
    {
        // Popup message window
        private Snackbar MainWndSnackbar = null;

        // Allow changes to AI method
        private volatile bool AllowAIUpdates = true;

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
            GetCurrentFaceAIDetection();
        }

        // Get the current server settings for face detection AI
        void GetCurrentFaceAIDetection()
        {
            Task.Run(() =>
            {
                // Create a message payload
                var items = new Dictionary<string, object>();
                items["Command"] = "GetAIMethod";
                var msg = Message.CreateMessageFromJson("ServerCommand", Message.MessageType.ServerCommand, items);

                // Send command
                var cmd = new SendCommand();
                bool bOK = cmd.SendCommandMessage(ref msg, out string error);
                Debug.Assert(bOK, error);
                if(bOK)
                {
                    msg.GetHeaderMapValue("AIMethod", out object value);
                    string method = Convert.ToString(value);
                    AllowAIUpdates = false;

                    // Set correct choice
                    if (method == "Off")
                        FaceAiOff = true;
                    else if (method == "OpenCV")
                        FaceAiOpenCV = true;
                    else if (method == "Dnn")
                        FaceAiDnn = true;
                    else if (method == "Mod")
                        FaceAiMod = true;
                    else if (method == "Hog")
                        FaceAiHog = true;
                    AllowAIUpdates = true;
                }
            });
        }

        // Update AI detection method on server
        void UpdateFaceAIDetection(string method)
        {
            if (AllowAIUpdates)
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
                    bool bOK = cmd.SendCommandMessage(ref msg, out string error);
                    Debug.Assert(bOK, error);
                });
            }
        }
    }
}
