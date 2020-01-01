using System;
using System.Collections.Generic;
using System.Text;

namespace MessagesLibrary
{
    public class MessageFactory
    {
		// Create message from key value pairs
		public static Message Create(string topic, Int32 type, Dictionary<string, object> items)
		{
			var msg = new Message();

			// Set topic and type
			msg.SetTopic(topic);
			msg.SetMessageType(type);
			msg.SetMicroTime();
			msg.HeaderMap.AddDictionary(items);
			return msg;
		}

		// Create message from data buffer
		public static Message Create(string topic, Int32 type, byte[] buffer)
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
	}
}
