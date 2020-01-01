using MessagesLibrary;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Unittest
{
    class TestJson
    {
        [SetUp]
        protected void SetUp()
        {
        }

        [TearDown]
        public void BaseTearDown()
        {
        }

        // Test simple map serialization std::any to Json
        [Test]
        public void TestMapSerialize()
        {
            var map1 = new Dictionary<string, object>();
            var map2 = new Dictionary<string, object>();
            map1["state"] = true;
            map1["sensor"] = "motion";

            JObject json = new JObject();
            Message.SerializeHeaderMapToJson(map1, ref json);
            MessageHelper.LoadJsonIntoMap(json, ref map2);

            Assert.True(map1.Count == map2.Count, "TestMapSerialize failed test.");
        }

        // Test map with recursive calls serialization std::any to Json
        [Test]
        public void TestMapSerializeSubmaps()
        {
            var subMap = new Dictionary<string, object>();
            subMap["state"] = true;
            subMap["sensor"] = "motion";

            var map1 = new Dictionary<string, object>();
            var map2 = new Dictionary<string, object>();
            map1["timestamp"] = 1589283928;
            map1["name"] = "unittest";
            map1["sub_items"] = subMap;

            JObject json = new JObject();
            Message.SerializeHeaderMapToJson(map1, ref json);
            MessageHelper.LoadJsonIntoMap(json, ref map2);

            Assert.True(map1.Count == map2.Count, "TestMapSerialize failed test.");
        }

        // Test serialization from map to byte[] buffer
        [Test]
        public void TestWriteJsonBufferMatches()
        {
            var values = new Dictionary<string, object>();
            values["state"] = true;
            values["sensor"] = "motion";
            Message message1 = MessageFactory.Create("unitTest", (Int32)Message.MessageType.MotionSensor, values);

            message1.SerializeMessageToBuffer(out byte[] data);
            Message message2 = Message.DeserializeBufferToMessage(data);

            Assert.True(message1 == message2, "TestWriteJsonBuffer failed == operator test.");

            // Add custom header data
            message1.SetHeaderMapValue("is_key_frame", true);
            message1.SerializeMessageToBuffer(out byte[] data2);
            message2 = Message.DeserializeBufferToMessage(data2);
            Assert.True(message1 == message2, "TestWriteJsonBuffer failed == operator test.");
        }

        // Test message overloaded operators
        [Test]
        public void TestOperatorOverload()
        {
            var values = new Dictionary<string, object>();
            values["state"] = false;
            values["sensor"] = "motion";
            Message message1 = MessageFactory.Create("unitTest", (Int32)Message.MessageType.MotionSensor, values);

            // Add custom header data
            message1.SetHeaderMapValue("is_key_frame", true);

            // Add data buffer a, b, c
            byte[] buffer = { 0x61, 0x62, 0x63 };
            message1.SetData(buffer);

            Message message2 = Message.DeepClone(message1);
            Assert.True(message1 == message2, "TestOperatorOverload failed == operator test.");

            // Make sure json data size is correct
            message2.GetHeaderMapValue("data_size", out object size);
            Assert.True((int)size == 3);

            byte[] data = message2.GetData();
            data[1] = 0x61;
            message2.SetData(data);

            // Detect that data buffers do not match
            Assert.False(message1 == message2, "TestOperatorOverload failed == operator test.");
        }

        // Test json config file
        [Test]
        public void TestJsonConfigFile()
        {
            // Load settings map from json config file
            string path = Helpers.AppendToRunPath(@"..\..\..\assets\message_developer.json");
            var settingsMap = MessageHelper.LoadSettingsFromConfig(path);

            var publisher = (Dictionary<string, object>)settingsMap["Publisher"];
            var uris = (List<object>)publisher["Endpoints"];

            string uri = uris[0].ToString();
            Assert.True(uris.Count == 1 && uri == "tcp://*:5563", "TestJsonConfigFile failed publisher test.");

            var subscriber = (Dictionary<string, object>)settingsMap["Subscriber"];
            uris = (List<object>)subscriber["Endpoints"];
            uri = uris[0].ToString();
            Assert.True(uris.Count == 1 && uri == "tcp://127.0.0.1:5563", "TestJsonConfigFile failed publisher test.");
        }

        // Test serialization with data buffer
        [Test]
        public void TestSerializeWithBufferMatches()
        {
            var values = new Dictionary<string, object>();
            values["state"] = true;
            values["sensor"] = "motion";
            Message message1 = MessageFactory.Create("unitTest", (Int32)Message.MessageType.MotionSensor, values);
            byte[] buffer = { 0x61, 0x62, 0x63 };
            message1.SetData(buffer);

            message1.SerializeMessageToBuffer(out byte[] data);
            Message message2 = Message.DeserializeBufferToMessage(data);

            Assert.True(message1 == message2, "TestSerializeWithBufferMatches failed == operator test.");

            // Add custom header data
            message1.SetHeaderMapValue("is_key_frame", true);
            message1.SerializeMessageToBuffer(out byte[] data2);
            message2 = Message.DeserializeBufferToMessage(data2);
            Assert.True(message1 == message2, "TestSerializeWithBufferMatches failed == operator test.");
        }
    }
}
