using System;
using tetryds.RealtimeMessaging.MemoryManagement;

namespace tetryds.RealtimeMessaging.Consumers
{
    public class EchoConsumer : IMessageConsumer
    {
        public event Action<Message> Replied;

        public void Consume(Message message)
        {
            Replied?.Invoke(message);
        }
    }
}
