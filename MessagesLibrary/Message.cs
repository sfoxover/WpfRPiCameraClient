using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

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
		Dictionary<string, object> _headerMap;

		// Message payload
		byte[] _data;

		public Message()
		{
			SetMicroTime();
			_headerMap["version"] = MESSAGE_VERSION;
		}

		public Message(byte[] data, Dictionary<string, object> map)
		{
			SetMicroTime();
			_headerMap["version"] = MESSAGE_VERSION;
			_data = data;
			_headerMap = map;
		}

		// Overloaded operators
		public static bool operator == (Message value, Message value2)
		{
			if (value._data.Length != value2._data.Length)
				return false;
			if (value._data.Length == 0)
				return true;

			// Convert header map to json to test
			SerializeHeaderMapToJson(value._headerMap, out JObject json1);
			SerializeHeaderMapToJson(value2._headerMap, out JObject json2);
			if (json1 != json2)
				return false;
			bool equal = value._data == value2._data;
			return equal;
		}
		public static bool operator != (Message value, Message value2)
		{
			bool equal = value == value2;
			return !equal;
		}

		public void SetMicroTime()
		{
			_headerMap["time_stamp"] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		}


		// Get Set a header key value pair in _headerMap
		void GetHeaderMapValue(string key, out object value)
		{
			value = _headerMap[key];
		}

		void SetHeaderMapValue(string key, object value)
		{
			_headerMap[key] = value;
		}

		// Get Set for _data
		byte[] GetData()
		{
			return _data;
		}

		void SetData(byte[] value)
		{
			_headerMap["data_size"] = value.Length;
			_data = value;
		}

		// Get Set for _type
		MessageType GetMessageType()
		{
			MessageType type = (MessageType)_headerMap["type"];
			return type;
		}

		void SetMessageType(MessageType value)
		{
			_headerMap["type"] = value;
		}

		// Get Set for _topic
		string GetTopic()
		{
			var value = (string)_headerMap["topic"];
			return value;
		}

		void SetTopic(string value)
		{
			_headerMap["topic"] = value;
		}

		// Get Set for _microTimeStamp
		Int64 GetMicroTimeStamp()
		{
			var time = (Int64)_headerMap["time_stamp"];
			return time;
		}

		void SetMicroTimeStamp(Int64 value)
		{
			_headerMap["time_stamp"] = value;
		}

		// Create message from key value pairs
		void CreateMessageFromJson(string topic, MessageType type, Dictionary<string, object> items)
		{
			// Set topic and type
			SetTopic(topic);
			SetMessageType(type);
			SetMicroTime();

			_headerMap["map_payload"] = items;
		}

		// Set message values including data buffer
		void CreateMessageFromBuffer(string topic, MessageType type, byte[] buffer)
		{
			// Set topic and type
			SetTopic(topic);
			SetMessageType(type);
			SetMicroTime();

			// Copy to buffer
			SetData(buffer);
		}

		// Deserialize buffer into message properties, topic + magic marker + message json + marker end + data
		void DeserializeBufferToMessage(byte[] buffer)
		{
			buffer = null;
			SetMessageType(MessageType.Unknown);

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
					MessageHelper::LoadJsonIntoMap(root, _headerMap);

					// Anything left over is the data buffer
					posEnd += MESSAGE_MARKER_END.Length;
					if(posEnd < buffer.Length)
					{
						byte[] data = new byte[buffer.Length - posEnd];
						buffer.CopyTo(data, );
						SetData(data);
					}
				#if DEBUG
					// Test topic string value
					string szTopic(buffer.begin(), posStart);
					string szTopicJson;
					GetTopic(szTopicJson);
					assert(!szTopic.empty() && szTopic == szTopicJson);
				#endif // DEBUG
				}
			}
		}

	// Serialize message properties topic + magic marker + message type + micro seconds + message to buffer
	void SerializeMessageToBuffer(out byte[] buffer)
	{
		// Write message topic
		string topic;
		GetTopic(topic);
		assert(!topic.empty());
		string strMessage = topic;

		// Add header start marker
		strMessage += string((const char*)MESSAGE_MARKER_START, sizeof(MESSAGE_MARKER_START));

		// Add header json values
		Json::Value root;
		SerializeHeaderMapToJson(_headerMap, root);
		Json::StreamWriterBuilder builder;
		const string json = Json::writeString(builder, root);
		strMessage += json;

		// Add header end marker
		strMessage += string((const char*)MESSAGE_MARKER_END, sizeof(MESSAGE_MARKER_END));

		// Convert header to vector
		auto tempData = vector < unsigned char> (strMessage.c_str(), strMessage.c_str() + strMessage.size());

		// Append message buffer data
		tempData.insert(tempData.end(), _data.begin(), _data.end());
		buffer = move(tempData);
	}

		// Convert header map to json
		void SerializeHeaderMapToJson(Dictionary<string, object> map, ref JObject json)
		{
			foreach(var item in map) 
			{
				if (item.GetType() == GetType(Dictionary<string, object>))
				{
					JObject doc = new JObject();
					SerializeHeaderMapToJson(any_cast < map < string, any >> (item.second), doc);
					json[item] = doc;
				}
				else
				{
					json[item.first] = MessageHelper::AnyValueToJson(item.second);
				}
			});
		}
}