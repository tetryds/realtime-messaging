using System;
using System.Collections.Generic;
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

            EchoConsumer echo = new EchoConsumer();
            echo.Replied += m => DoRespond(m, true);

            consumerMap = new Dictionary<char, IMessageConsumer>()
            {
                ['E'] = echo
            };
        }

        public void Start()
        {
            routerWorker = new Thread(DoRoute);
            routerWorker.IsBackground = true;
            routerWorker.Priority = ThreadPriority.BelowNormal;
            routerWorker.Start();

            gateway.Start();

            running = true;
        }

        public bool HasConsumer(char key) => consumerMap.ContainsKey(key);

        public bool TryRegisterConsumer(char key, IMessageConsumer consumer)
        {
            if (HasConsumer(key)) return false;

            consumerMap.Add(key, consumer);
            return true;
        }

        private void DoRoute()
        {
            while (running)
            {
                if (!gateway.TryGet(PollingSkipRateMs, out Message message)) continue;

                if (!consumerMap.TryGetValue(message.Type, out IMessageConsumer consumer))
                {
                    ErrorOcurred?.Invoke(new Exception($"Unmapped message type '{message.Type}' cannot be consumed"));
                }

                consumer.Consume(message);
            }
        }

        private void DoRespond(Message message, bool raiseError)
        {
            try
            {
                gateway.Send(message);
            }
            catch (Exception e)
            {
                if (raiseError)
                    ErrorOcurred?.Invoke(e);
            }
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
