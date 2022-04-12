using System;
using System.Text;
using tetryds.RealtimeMessaging.MemoryManagement;

namespace tetryds.RealtimeMessaging
{
    public class ThrowingConsumer : IMessageConsumer
    {
        public event Action<Message> Replied;

        public void Consume(Message message)
        {
            string errorMessage = Encoding.UTF8.GetString(message.Data);
            throw new Exception(errorMessage);
        }
    }
}
