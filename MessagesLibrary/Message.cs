using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace MessagesLibrary
{
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
			Unknown,
			VideoCam,
			VideoSample,
			FaceDetection,
			Audio,
			MotionSensor,
			Other
		};

		// A map of any types that gets converted to and from json
		Dictionary<string, object> HeaderMap;

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

		public Message DeepCopy()
		{
			Message msg = new Message();
			msg.HeaderMap = HeaderMap.ToDictionary(kv => kv.Key, kv => kv.Value.Clone() as object);

			msg.Data = new byte[Data.Length];
			Data.CopyTo(msg.Data, 0);
			return msg;
		}


		// Overloaded operators
		public static bool operator ==(Message value, Message value2)
		{
			if (value.Data.Length != value2.Data.Length)
				return false;
			if (value.Data.Length == 0)
				return true;

			// Convert header map to json to test
			var json1 = new JObject();
			var json2 = new JObject();
			SerializeHeaderMapToJson(value.HeaderMap, ref json1);
			SerializeHeaderMapToJson(value2.HeaderMap, ref json2);
			if (!JToken.DeepEquals(json1, json2))
				return false;
			bool equal = value.Data.SequenceEqual(value2.Data);
			return equal;
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

		// Get Set for _type
		public MessageType GetMessageType()
		{
			MessageType type = (MessageType)HeaderMap["type"];
			return type;
		}

		public void SetMessageType(MessageType value)
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

		// Create message from key value pairs
		public static Message CreateMessageFromJson(string topic, MessageType type, Dictionary<string, object> items)
		{
			var msg = new Message();

			// Set topic and type
			msg.SetTopic(topic);
			msg.SetMessageType(type);
			msg.SetMicroTime();
			msg.HeaderMap["map_payload"] = items;
			return msg;
		}

		// Set message values including data buffer
		public static Message CreateMessageFromBuffer(string topic, MessageType type, byte[] buffer)
		{
			Message msg = new Message();

			// Set topic and type
			msg.SetTopic(topic);
			msg.SetMessageType(type);
			msg.SetMicroTime();

			// Copy to buffer
			msg.SetData(buffer);
			return msg;
		}

		// Deserialize buffer into message properties, topic + magic marker + message json + marker end + data
		public static Message DeserializeBufferToMessage(byte[] buffer)
		{
			Message msg = new Message();
			msg.SetMessageType(MessageType.Unknown);

			// Search for start marker after topic
			var posStart = Array.IndexOf(buffer, MESSAGE_MARKER_START);

			// We found start marker so look for end marker
			if (posStart < MAX_TOPIC_LENGTH)
			{
				posStart += MESSAGE_MARKER_START.Length;
				var posEnd = Array.IndexOf(buffer, MESSAGE_MARKER_END);
				if (posEnd != -1)
				{
					// Load json values into header map
					string szJson = UTF8Encoding.UTF8.GetString(buffer, posStart, posEnd);
					JObject root = JObject.Parse(szJson);

					// Load all json values into our header map
					MessageHelper.LoadJsonIntoMap(root, ref msg.HeaderMap);

					// Anything left over is the data buffer
					posEnd += MESSAGE_MARKER_END.Length;
					if (posEnd < buffer.Length)
					{
						byte[] data = new byte[buffer.Length - posEnd];
						buffer.CopyTo(data, posEnd);
						msg.SetData(data);
					}
				#if DEBUG
					// Test topic string value
					string szTopic = UTF8Encoding.UTF8.GetString(buffer, 0, posStart);
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
			string strMessage = topic;

			// Add header start marker
			strMessage += MESSAGE_MARKER_START;

			// Add header json values
			JObject doc = new JObject();
			SerializeHeaderMapToJson(HeaderMap, ref doc);
			string json = doc.ToString();
			strMessage += json;

			// Add header end marker
			strMessage += MESSAGE_MARKER_END;

			// Convert header to vector
			buffer = UTF8Encoding.UTF8.GetBytes(strMessage);

			// Append message buffer data
			System.Buffer.BlockCopy(Data, 0, buffer, buffer.Length, Data.Length);
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