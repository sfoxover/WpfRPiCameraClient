using System;
using System.Collections.Generic;
using System.Diagnostics;

/*
    Settings - class to load and retrieve various settings from json config file
*/

namespace MessagesLibrary
{
    public class Settings
    {
        // Singleton access to this object
        private static readonly Lazy<Settings> lazy = new Lazy<Settings>(() => new Settings());
        public static Settings Instance { get { return lazy.Value; } }

        // Message publish endpoints
        public string PublishUri { get; set; }

        // Message subscribe endpoints
        public string SubscribeUri { get; set; }

        // Command server endpoint
        public string CmdServerUri { get; set; }

        // Command client endpoint
        public string CmdClientUri { get; set; }

        // Video settings
        bool UseSampleVideo { get; set; }
        string SampleVideoName { get; set; }

        // Face detect settings
        public bool UseFaceDetect { get; set; }
        public string FaceDetectMethod { get; set; }

        // Topic settings
        public string VideoCamTopic { get; set; }
        public string VideoSampleTopic { get; set; }
        public string FaceDetectTopic { get; set; }
        public string MotionSensor { get; set; }
        public string ProfilingTopic { get; set; }

        // Private for singleton
        private Settings()
        {
        }

        // Initialize all settings
        public bool Initialize(string jsonPath, out string error)
        {
            error = "";
            try
            {
                // Load json settings values
                var settingsMap = MessageHelper.LoadSettingsFromConfig(jsonPath);

                // Load publisher endpoint
                PublishUri = (string)settingsMap["PublisherEndpoint"];

                // Subscriber endpoint
                SubscribeUri = (string)settingsMap["SubscriberEndpoint"];

                // Command server endpoint
                CmdServerUri = (string)settingsMap["CmdServerEndpoint"];

                // Command client endpoint
                CmdClientUri = (string)settingsMap["CmdClientEndpoint"];

                // Set face detection settings
                Dictionary<string, object> videoSettings = (Dictionary<string, object>)settingsMap["VideoSettings"];
                UseSampleVideo = (bool)videoSettings["StreamSampleVideo"];
                SampleVideoName = (string)videoSettings["SampleVideoName"];
                UseFaceDetect = (bool)videoSettings["RunFaceDetection"];
                FaceDetectMethod = (string)videoSettings["FaceDetectionMethod"];

                // Topic settings
                VideoCamTopic = (string)settingsMap["VideoCamTopic"];
                VideoSampleTopic = (string)settingsMap["VideoSampleTopic"];
                FaceDetectTopic = (string)settingsMap["FaceDetectTopic"];
                MotionSensor = (string)settingsMap["MotionSensorTopic"];
                ProfilingTopic = (string)settingsMap["ProfilingTopic"];

                return true;
            }
            catch (Exception ex)
            {
                error = $"Settings::Initialize error {ex.Message}";
                Debug.WriteLine(error); 
            }
            return false;
        }
    }
}
