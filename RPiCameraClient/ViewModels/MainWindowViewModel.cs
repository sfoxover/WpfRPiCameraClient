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

        // Live video playback toggle button
        private bool _liveVideoPlayOn = true;
        public bool LiveVideoPlayOn
        {
            get { return _liveVideoPlayOn; }
            set
            {
                if (SetProperty(ref _liveVideoPlayOn, value))
                    UpdateLiveVideoPlayback(!_liveVideoPlayOn);
            }
        }

        // Show or hide video player window
        private bool _videoPlayVisible = true;
        public bool VideoPlayVisible
        {
            get { return _videoPlayVisible; }
            set { SetProperty(ref _videoPlayVisible, value); }
        }

        // Show or hide face detection video window
        private bool _facePlayVisible = true;
        public bool FacePlayVisible
        {
            get { return _facePlayVisible; }
            set { SetProperty(ref _facePlayVisible, value); }
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
            GetCurrentSettings();            
        }

        // Get the current server settings 
        async Task GetCurrentSettings()
        {
            await Task.Run(() =>
            {
                // Create a message payload
                var items = new Dictionary<string, object>();
                items["Command"] = "GetCurrentSettings";
                var msg = MessageFactory.Create("ServerCommand", (Int32)Message.MessageType.ServerCommand, items);

                // Send command
                var cmd = new SendCommand();
                bool bOK = cmd.SendCommandMessage(ref msg, out string error);
                Debug.Assert(bOK, error);
                if (bOK)
                {
                    // Read all json setting into object map
                    msg.GetHeaderMapValue("Settings", out object value);
                    Dictionary<string, object> settingsMap = value as Dictionary<string, object>;

                    // Do not send UI changes to the server while view model is synced with current settings
                    AllowUICommandUpdates = false;

                    // Get facial detection method
                    string method = settingsMap["AIMethod"] as string;
                    bool faceWindowVisible = true;
                    if (method == "Off")
                    {
                        FaceAiOff = true;
                        faceWindowVisible = false;
                    }
                    else if (method == "OpenCV")
                        FaceAiOpenCV = true;
                    else if (method == "Dnn")
                        FaceAiDnn = true;
                    else if (method == "Mod")
                        FaceAiMod = true;
                    else if (method == "Hog")
                        FaceAiHog = true;

                    // Update video window visibility
                    FacePlayVisible = faceWindowVisible;

                    // Get is video playing
                    VideoPlayOn = Convert.ToBoolean(settingsMap["VideoPlaying"]);

                    // Get is live video playing
                    bool livePlaying = !Convert.ToBoolean(settingsMap["UseSampleVideoFile"]);
                    LiveVideoPlayOn = livePlaying;

                    AllowUICommandUpdates = true;

                    ShowSnackMessage($"Face detection is {method}, live video is {LiveVideoPlayOn}.");
                }
                else
                {
                    ShowSnackMessage($"Failed to connect to server {Settings.Instance.CmdClientUri}.");
                }
            });
        }

        // Update AI detection method on server
        async Task UpdateFaceAIDetection(string method)
        {
            if (AllowUICommandUpdates)
            {
                await Task.Run(() =>
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

                // Do settings check to verify change
                await GetCurrentSettings();
            }
        }

        // Toggle live video playback on or off
        void UpdateLiveVideoPlayback(bool useSample)
        {
            if (AllowUICommandUpdates)
            {
                Task.Run(() =>
                {
                    // Create a message payload
                    var items = new Dictionary<string, object>();
                    items["Command"] = "SetUseSampleVideo";
                    items["Value"] = useSample;
                    var msg = MessageFactory.Create("ServerCommand", (Int32)Message.MessageType.ServerCommand, items);

                    // Send command
                    var cmd = new SendCommand();
                    bool bOK = cmd.SendCommandMessage(ref msg, out string error);
                    Debug.Assert(bOK, error);
                });
            }
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
