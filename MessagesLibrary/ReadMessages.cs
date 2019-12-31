using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using ZeroMQ;
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
            Task.Run(() => ReadMessagesThread());
        }

        public void Stop()
        {
            WaitEvent.Set();
        }

        void ReadMessagesThread()
        {
            string uri = Settings.Instance.SubscribeUri;

            using (var subscriber = new ZSocket(ZSocketType.SUB))
            {
                subscriber.ReceiveTimeout = new TimeSpan(0, 0, 5);
                subscriber.Connect(uri);
                foreach (var topic in Topics)
                {
                    subscriber.Subscribe(topic);
                }

                do
                {
                    try
                    {
                        using (ZFrame reply = subscriber.ReceiveFrame())
                        {
                            byte[] tempBuffer = reply.Read();
                            var msg = Message.DeserializeBufferToMessage(tempBuffer);
                            MessageCallback?.Invoke(msg);
                        }
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine($"ReadMessagesThread Exception {ex.Message}");
                    }
                }
                while (WaitEvent.WaitOne(1) == false);
            }
        }
    }
}
