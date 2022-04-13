using System;
using System.Linq;
using System.Text;
using tetryds.RealtimeMessaging.MemoryManagement;

namespace tetryds.RealtimeMessaging.Consumers
{
    public class InfoConsumer : IMessageConsumer
    {
        Router router;

        public event Action<Message> Replied;

        public InfoConsumer(Router router)
        {
            this.router = router;
        }

        public void Consume(Message message)
        {
            string request = Encoding.UTF8.GetString(message.Data);
            string info = "";

            if (request == "Consumers")
                info = ConsumersInfo();

            Message reply = new Message();
            reply.Status = Status.Ok;
            reply.Type = message.Type;
            reply.Data = Encoding.UTF8.GetBytes(info);
        }

        private string ConsumersInfo()
        {
            return string.Join("; ", router.GetConsumers().Select((key, type) => $"{key}, {type}"));
        }
    }
}
