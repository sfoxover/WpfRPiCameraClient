using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MessagesLibrary
{
    // Send a command message to the connected device
    public class SendCommand
    {
		public SendCommand()
		{
		}

		// Send message to the command server
		public bool SendCommandMessage(ref Message msg, out string error)
		{
			error = "";
			try
			{
				bool bOK = false;
				// Load message topic and data
				msg.SerializeMessageToBuffer(out byte[] buffer);

				// Send to command server endpoint
				using (var client = new RequestSocket())
				{
					client.Connect(Settings.Instance.CmdClientUri);
					bOK = client.TrySendFrame(new TimeSpan(0, 0, 5), buffer);

					if (bOK)
					{
						bOK = client.TryReceiveFrameBytes(new TimeSpan(0, 0, 5), out byte[] resultBuffer, out bool more);
						Debug.Assert(!more, "Errro, SendCommandMessage reply has more data to receive.");
						if (bOK)
						{
							msg = Message.DeserializeBufferToMessage(resultBuffer);
						}
					}
				}
				return bOK;
			}
			catch(Exception ex)
			{
				error = $"SendCommandMessage error {ex.Message}";
				Debug.WriteLine(error);
			}
			return false;
		}
	}
}
