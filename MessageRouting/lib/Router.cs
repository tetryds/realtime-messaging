using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace tetryds.RealtimeMessaging
{
    public class Router : IDisposable
    {
        public int PollingSkipRateMs { get; set; } = 10;

        IGateway<Message> gateway;

        Dictionary<char, IMessageConsumer> consumerMap;

        volatile bool disposed = false;
        volatile bool running = false;

        Thread routerWorker;

        public event Action<Exception> ErrorOcurred;

        public Router(IGateway<Message> gateway)
        {
            this.gateway = gateway;
            consumerMap = new Dictionary<char, IMessageConsumer>();
        }

        public void Start()
        {
            if (running) return;

            routerWorker = new Thread(DoRoute);
            routerWorker.IsBackground = true;
            routerWorker.Priority = ThreadPriority.BelowNormal;

            gateway.Start();
            routerWorker.Start();

            running = true;
        }

        public bool HasConsumer(char key) => consumerMap.ContainsKey(key);

        public bool TryRegisterConsumer(char key, IMessageConsumer consumer) => TryRegisterConsumer(key, consumer, false);

        public bool TryRegisterConsumer(char key, IMessageConsumer consumer, bool supressError)
        {
            if (HasConsumer(key)) return false;

            consumer.Replied += supressError ? (Action<Message>)DoRespondSupressError : DoRespond;

            consumerMap.Add(key, consumer);
            return true;
        }

        public List<(char, Type)> GetConsumers() => consumerMap.Select(kvp => (kvp.Key, kvp.Value.GetType())).ToList();

        private void DoRoute()
        {
            while (running)
            {
                if (!gateway.TryGet(PollingSkipRateMs, out Message message)) continue;

                try
                {
                    if (!consumerMap.TryGetValue(message.Type, out IMessageConsumer consumer))
                        throw new Exception($"Unmapped message type '{message.Type}', message cannot be consumed");

                    consumer.Consume(message);
                }
                catch (Exception e)
                {
                    ErrorOcurred?.Invoke(e);
                }
            }
        }

        private void DoRespond(Message message)
        {
            try
            {
                gateway.Send(message);
            }
            catch (Exception e)
            {
                ErrorOcurred?.Invoke(e);
            }
        }

        private void DoRespondSupressError(Message message)
        {
            try
            {
                gateway.Send(message);
            }
            catch { }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            running = false;

            if (gateway is IDisposable disposableGateway)
                disposableGateway.Dispose();

            foreach (char consumerId in consumerMap.Keys)
            {
                if (consumerMap[consumerId] is IDisposable disposableConsumer)
                    disposableConsumer.Dispose();
            }

            routerWorker?.Join(PollingSkipRateMs + 10);
        }
    }
}
