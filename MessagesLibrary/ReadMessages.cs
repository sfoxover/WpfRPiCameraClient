using System;
using NetMQ;
using NetMQ.Sockets;
using System.Threading.Tasks;
using System.Threading;

namespace MessagesLibrary
{
    class ReadMessages
    {
        private static AutoResetEvent WaitEvent = new AutoResetEvent(false);

        void Start()
        {
            using (var runtime = new NetMQRuntime())
            {
                runtime.Run(ClientAsync());
            }           
        }

        async Task ClientAsync()
        {
            string uri = "tcp://10.0.0.4:5563";
            using (var subscriber = new SubscriberSocket(uri))
            {
                subscriber.Options.ReceiveHighWatermark = 1000;
                subscriber.Connect(uri);
                subscriber.Subscribe("VideoCam");
                do 
                { 
                    var (message, more) = await subscriber.ReceiveFrameBytesAsync();
                }
                while (WaitEvent.WaitOne(10) == false);
            }
        }
    }
}
