using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace tetryds.RealtimeMessaging
{
    public class Client : IDisposable
    {
        IGateway<Message> gateway;

        volatile bool disposed = false;
        volatile bool running = false;

        public MessageHandler(IGateway<Message> gateway)
        {
            this.gateway = gateway;
        }

        public void Start()
        {
            if (running) return;

            gateway.Start();

            running = true;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            running = false;

            if (gateway is IDisposable disposableGateway)
                disposableGateway.Dispose();
        }
    }
}
