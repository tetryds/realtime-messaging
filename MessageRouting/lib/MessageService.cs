using System;
using System.Collections.Generic;
using System.Threading;

namespace tetryds.RealtimeMessaging
{
    public class MessageService : IDisposable
    {
        Router router;

        volatile bool disposed = false;
        volatile bool running = false;

        public event Action<Exception> ErrorOcurred;

        public MessageService(IGateway<Message> gateway, bool defaultConsumers)
        {
            router = new Router(gateway);

            if (defaultConsumers)
                RegisterDefaultConsumers();
        }

        public void Start()
        {
            if (running) return;

            router.Start();

            running = true;
        }

        public bool HasConsumer(char key) => router.HasConsumer(key);

        public bool TryRegisterConsumer(char key, IMessageConsumer consumer, bool signalError)
        {
            return router.TryRegisterConsumer(key, consumer, signalError);
        }

        private void RegisterDefaultConsumers()
        {
            EchoConsumer echo = new EchoConsumer();
            router.TryRegisterConsumer('E', echo, true);

            ErrorConsumer error = new ErrorConsumer();
            router.ErrorOcurred += error.NotifyError;
            router.TryRegisterConsumer('R', error, true);

            ThrowingConsumer throwing = new ThrowingConsumer();
            router.TryRegisterConsumer('W', throwing, true);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            router.Dispose();
            running = false;
        }
    }
}
