using System;
using NetMQ;
using NetMQ.Sockets;
using System.Threading.Tasks;
using System.Threading;

namespace MessagesLibrary
{
    public class ReadMessages
    {
        // Stop event
        private static AutoResetEvent WaitEvent = new AutoResetEvent(false);

        // New message callback delegate
        public delegate void NewMessageDelegate(Message msg);
        public NewMessageDelegate MessageCallback { get; set; }

        public ReadMessages()
        {
            MessageCallback = null;
        }

        public void Start()
        {
            using (var runtime = new NetMQRuntime())
            {
                runtime.Run(ReadMessagesAsync());
            }           
        }

        public void Stop()
        {
            WaitEvent.Set();
        }

        async Task ReadMessagesAsync()
        {
            string uri = "tcp://127.0.0.1:5563";
            using (var subscriber = new SubscriberSocket(uri))
            {
                subscriber.Options.ReceiveHighWatermark = 1000;
                subscriber.Connect(uri);
                subscriber.Subscribe("VideoCam");
                subscriber.Subscribe("VideoSample");

                byte[] tempBuffer = null;
                do 
                { 
                    var (buffer, more) = await subscriber.ReceiveFrameBytesAsync();
                    tempBuffer = tempBuffer.AppendBytes(buffer);
                    if (!more)
                    {
                        var msg = Message.DeserializeBufferToMessage(tempBuffer);
                        tempBuffer = null;
                        MessageCallback?.Invoke(msg);
                    }
                }
                while (WaitEvent.WaitOne(10) == false);
            }
        }
    }
}
