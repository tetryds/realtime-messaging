using System;
using tetryds.RealtimeMessaging.MemoryManagement;

namespace tetryds.RealtimeMessaging
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
