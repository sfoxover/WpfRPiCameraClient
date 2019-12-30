using System;
using NetMQ;
using NetMQ.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace MessagesLibrary
{
    public class ReadMessages
    {
        // Stop event
        private static AutoResetEvent WaitEvent = new AutoResetEvent(false);

        // New message callback delegate
        public delegate void NewMessageDelegate(Message msg);
        public NewMessageDelegate MessageCallback { get; set; }

        // Topics to subscribe to
        public List<string> Topics { get; set; }

        public ReadMessages(List<string> topics)
        {
            MessageCallback = null;
            Topics = topics;
        }

        public void Start()
        {
            var test = new SocketLibrary.Class1();

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
            string uri = Settings.Instance.SubscribeUri;
            using (var subscriber = new SubscriberSocket(uri))
            {
                subscriber.Options.ReceiveHighWatermark = 1000;
                subscriber.Connect(uri);
                foreach (var topic in Topics)
                {
                    subscriber.Subscribe(topic);
                }

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
                        Debug.WriteLine("Got frame");
                    }
                }
                while (WaitEvent.WaitOne(10) == false);
            }
        }
    }
}
