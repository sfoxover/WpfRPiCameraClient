using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ZeroMQ;

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
				using (var client = new ZSocket(ZSocketType.REQ))
				{
					client.SendTimeout = new TimeSpan(0, 0, 5);
					client.ReceiveTimeout = new TimeSpan(0, 0, 5);
					client.Connect(Settings.Instance.CmdClientUri);
					ZFrame frame = new ZFrame(buffer);
					bOK = client.Send(frame, out ZError zerror);
					if (bOK)
					{
						using (ZFrame reply = client.ReceiveFrame())
						{
							byte[] resultBuffer = reply.Read();
							msg = Message.DeserializeBufferToMessage(resultBuffer);
						}
					}
				}
				return bOK;
			}
			catch(Exception ex)
			{
				error = $"Settings::Initialize error {ex.Message}";
				Debug.WriteLine(error);
			}
			return false;
		}
	}
}
