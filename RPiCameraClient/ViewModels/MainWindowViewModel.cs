using MaterialDesignThemes.Wpf;
using MessagesLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RPiCameraClient.ViewModels
{
    internal class MainWindowViewModel : ViewModelHelper
    {
        // Popup message window
        private Snackbar MainWndSnackbar = null;

        // Allow changes to AI method
        private volatile bool AllowUICommandUpdates = true;

        // Video playback toggle button
        private bool _videoPlayOn = true;
        public bool VideoPlayOn
        {
            get { return _videoPlayOn; }
            set
            {
                if (SetProperty(ref _videoPlayOn, value))
                    UpdateVideoPlayback();
            }
        }

        // Face detection AI values: Off, OpenCV, Dnn, Mod, Hog
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
            GetVideoPlaying();
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
                var msg = MessageFactory.Create("ServerCommand", (Int32)Message.MessageType.ServerCommand, items);

                // Send command
                var cmd = new SendCommand();
                bool bOK = cmd.SendCommandMessage(ref msg, out string error);
                if(bOK)
                {
                    msg.GetHeaderMapValue("AIMethod", out object value);
                    string method = Convert.ToString(value);
                    AllowUICommandUpdates = false;

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
                    AllowUICommandUpdates = true;

                    ShowSnackMessage($"Server facial detection is currently {method}.");
                }
                else
                {
                    ShowSnackMessage($"Failed to connect to server {Settings.Instance.CmdClientUri}.");
                }
            });
        }

        // Update AI detection method on server
        void UpdateFaceAIDetection(string method)
        {
            if (AllowUICommandUpdates)
            {
                Task.Run(() =>
                {
                    // Create a message payload
                    var items = new Dictionary<string, object>();
                    items["Command"] = "SetAIMethod";
                    items["Value"] = method;
                    var msg = MessageFactory.Create("ServerCommand", (Int32)Message.MessageType.ServerCommand, items);

                    // Send command
                    var cmd = new SendCommand();
                    bool bOK = cmd.SendCommandMessage(ref msg, out string error);
                    Debug.Assert(bOK, error);
                });
            }
        }

        // Check if video is playing
        void GetVideoPlaying()
        {
            Task.Run(() =>
            {
                // Create a message payload
                var items = new Dictionary<string, object>();
                items["Command"] = "GetVideoPlaying";
                var msg = MessageFactory.Create("ServerCommand", (Int32)Message.MessageType.ServerCommand, items);

                // Send command
                var cmd = new SendCommand();
                bool bOK = cmd.SendCommandMessage(ref msg, out string error);
                if (bOK)
                {
                    msg.GetHeaderMapValue("VideoPlaying", out object value);
                    bool playing = Convert.ToBoolean(value);
                    AllowUICommandUpdates = false;
                    VideoPlayOn = playing;
                    AllowUICommandUpdates = true;
                }
            });
        }

        // Toggle video playback on or off
        void UpdateVideoPlayback()
        {
            if (AllowUICommandUpdates)
            {
                Task.Run(() =>
                {
                    // Create a message payload
                    var items = new Dictionary<string, object>();
                    if (VideoPlayOn)
                    {
                        items["Command"] = "StartVideo";
                    }
                    else
                    {
                        items["Command"] = "StopVideo";
                    }
                    var msg = MessageFactory.Create("ServerCommand", (Int32)Message.MessageType.ServerCommand, items);

                    // Send command
                    var cmd = new SendCommand();
                    bool bOK = cmd.SendCommandMessage(ref msg, out string error);
                    Debug.Assert(bOK, error);
                });
            }
        }

        void ShowSnackMessage(string msg)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainWndSnackbar.MessageQueue.Enqueue(msg);
            }));
        }
    }
}
