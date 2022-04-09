using System;
using tetryds.RealtimeMessaging.MemoryManagement;

namespace tetryds.RealtimeMessaging
{
    public interface IMessageConsumer
    {
        event Action<Message> Replied;
        void Consume(Message message);
    }
}
