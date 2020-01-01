using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MessagesLibrary
{
	[Serializable]
	public class Message 
	{
		// Message container version
		const string MESSAGE_VERSION = "1.0";

		// Max allowed size for a message topic
		const int MAX_TOPIC_LENGTH = 256;

		// Message header start marker sequence
		public static readonly byte[] MESSAGE_MARKER_START = { 0xD1, 0xFF, 0xD2, 0xFE };

		// Message header end marker sequence
		public static readonly byte[] MESSAGE_MARKER_END = { 0xD1, 0xFF, 0xD3, 0xFA };

		// Suported message types
		public enum MessageType
		{
			Unknown = 0,
			OpenCVMatFrame = 1,
			JpegFrame = 1 << 1,
			FaceDetection = 1 << 2,
			Video = 1 << 3,
			Audio = 1 << 4,
			MotionSensor = 1 << 5,
			ServerCommand = 1 << 6,
			ProfilingData = 1 << 7,
			Other = 1 << 8
		};

		// A map of any types that gets converted to and from json
		public Dictionary<string, object> HeaderMap;

		// Message payload
		byte[] Data;

		public Message()
		{
			Data = null;
			HeaderMap = new Dictionary<string, object>();
			SetMicroTime();
			HeaderMap["version"] = MESSAGE_VERSION;
		}

		public Message(byte[] data, Dictionary<string, object> map)
		{
			HeaderMap = new Dictionary<string, object>();
			SetMicroTime();
			HeaderMap["version"] = MESSAGE_VERSION;
			Data = data;
			HeaderMap = map;
		}

		public static T DeepClone<T>(T obj)
		{
			using (var ms = new MemoryStream())
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(ms, obj);
				ms.Position = 0;
				return (T)formatter.Deserialize(ms);
			}
		}

		// Overloaded operators
		public static bool operator ==(Message value, Message value2)
		{
			// Check for empty data values
			bool data1Empty = value.Data == null;
			bool data2Empty = value2.Data == null;
			if (data1Empty != data2Empty)
				return false;

			// Convert header map to json to test
			var json1 = new JObject();
			var json2 = new JObject();
			SerializeHeaderMapToJson(value.HeaderMap, ref json1);
			SerializeHeaderMapToJson(value2.HeaderMap, ref json2);
			if (!JToken.DeepEquals(json1, json2))
				return false;

			if (!data1Empty && !data2Empty)
			{
				if (value.Data.Length != value2.Data.Length)
					return false;
				if (value.Data.Length == 0)
					return true;
				bool equal = value.Data.SequenceEqual(value2.Data);
				return equal;
			}
			return true;
		}

		public static bool operator !=(Message value, Message value2)
		{
			bool equal = value == value2;
			return !equal;
		}

		public void SetMicroTime()
		{
			HeaderMap["time_stamp"] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		}


		// Get Set a header key value pair in HeaderMap
		public void GetHeaderMapValue(string key, out object value)
		{
			value = HeaderMap[key];
		}

		public void SetHeaderMapValue(string key, object value)
		{
			HeaderMap[key] = value;
		}

		// Get Set for Data
		public byte[] GetData()
		{
			return Data;
		}

		public void SetData(byte[] value)
		{
			HeaderMap["data_size"] = value.Length;
			Data = value;
		}

		public Int32 GetDataSize()
		{
			if (Data != null)
			{
				return Data.Count();
			}
			return 0;
		}

		// Get Set for _type
		public Int32 GetMessageType()
		{
			try
			{
				var type = Convert.ToInt32(HeaderMap["type"]);
				return type;
			}
			catch(Exception ex)
			{
				Debug.WriteLine($"GetMessageType exception {ex.Message}");
				return 0;
			}
		}

		public void SetMessageType(Int32 value)
		{
			HeaderMap["type"] = value;
		}

		// Get Set for _topic
		public string GetTopic()
		{
			var value = (string)HeaderMap["topic"];
			return value;
		}

		public void SetTopic(string value)
		{
			HeaderMap["topic"] = value;
		}

		// Get Set for _microTimeStamp
		public Int64 GetMicroTimeStamp()
		{
			var time = (Int64)HeaderMap["time_stamp"];
			return time;
		}

		public void SetMicroTimeStamp(Int64 value)
		{
			HeaderMap["time_stamp"] = value;
		}

		// Deserialize buffer into message properties, topic + magic marker + message json + marker end + data
		public static Message DeserializeBufferToMessage(byte[] buffer)
		{
			Message msg = new Message();
			msg.SetMessageType((Int32)MessageType.Unknown);

			// Search for start marker after topic
			var markerStart = Helpers.FindInArray(buffer, MESSAGE_MARKER_START);
			Debug.Assert(markerStart != -1);

			// We found start marker so look for end marker
			if (markerStart < MAX_TOPIC_LENGTH)
			{
				int posStart = markerStart + MESSAGE_MARKER_START.Length;
				var posEnd = Helpers.FindInArray(buffer, MESSAGE_MARKER_END);
				if (posEnd != -1)
				{
					// Load json values into header map
					string szJson = UTF8Encoding.UTF8.GetString(buffer, posStart, posEnd - posStart);
					JObject root = JObject.Parse(szJson);

					// Load all json values into our header map
					MessageHelper.LoadJsonIntoMap(root, ref msg.HeaderMap);

					// Anything left over is the data buffer
					posEnd += MESSAGE_MARKER_END.Length;
					if (posEnd < buffer.Length)
					{
						byte[] data = new byte[buffer.Length - posEnd];
						System.Buffer.BlockCopy(buffer, posEnd, data, 0, data.Length);
						msg.SetData(data);
					}
				#if DEBUG
					// Test topic string value
					string szTopic = UTF8Encoding.UTF8.GetString(buffer, 0, markerStart);
					string szTopicJson = msg.GetTopic();
					Debug.Assert(!string.IsNullOrEmpty(szTopic) && szTopic == szTopicJson);
				#endif // DEBUG
				}
			}
			return msg;
		}

		// Serialize message properties topic + magic marker + message type + micro seconds + message to buffer
		public void SerializeMessageToBuffer(out byte[] buffer)
		{
			// Write message topic
			string topic = GetTopic();
			Debug.Assert(!string.IsNullOrEmpty(topic));
			buffer = UTF8Encoding.UTF8.GetBytes(topic);

			// Add header start marker
			int endPos = buffer.Length;
			Array.Resize(ref buffer, endPos + MESSAGE_MARKER_START.Length);
			System.Buffer.BlockCopy(MESSAGE_MARKER_START, 0, buffer, endPos, MESSAGE_MARKER_START.Length);

			// Add header json values
			JObject doc = new JObject();
			SerializeHeaderMapToJson(HeaderMap, ref doc);
			byte[] jsonBytes = UTF8Encoding.UTF8.GetBytes(doc.ToString());
			endPos = buffer.Length;
			Array.Resize(ref buffer, endPos + jsonBytes.Length);
			System.Buffer.BlockCopy(jsonBytes, 0, buffer, endPos, jsonBytes.Length);

			// Add header end marker
			endPos = buffer.Length;
			Array.Resize(ref buffer, endPos + MESSAGE_MARKER_END.Length);
			System.Buffer.BlockCopy(MESSAGE_MARKER_END, 0, buffer, endPos, MESSAGE_MARKER_END.Length);

			// Append message buffer data
			if (Data != null && Data.Length > 0)
			{
				endPos = buffer.Length;
				Array.Resize(ref buffer, endPos + Data.Length);
				System.Buffer.BlockCopy(Data, 0, buffer, endPos, Data.Length);
			}
		}

		// Convert header map to json
		public static void SerializeHeaderMapToJson(Dictionary<string, object> map, ref JObject json)
		{
			Debug.Assert(json != null);
			foreach (var item in map)
			{
				if (item.GetType() == map.GetType())
				{
					JObject doc = new JObject();
					SerializeHeaderMapToJson((Dictionary<string, object>)item.Value, ref doc);
					json[item.Key] = doc;
				}
				else
				{
					json[item.Key] = MessageHelper.AnyValueToJson(item.Value);
				}
			}
		}
	}
}