using System;

namespace tetryds.RealtimeMessaging.Network
{
    public class SocketServerGateway<T> : IGateway<T> where T : IMessage, new()
    {
        int port;

        public SocketServerGateway(int port)
        {
            this.port = port;
        }

        public void Connect()
        {

        }

        public bool ReleaseId(int id)
        {
            throw new NotImplementedException();
        }

        public void Send(T message)
        {
            throw new NotImplementedException();
        }

        public bool TryGet(out T message)
        {
            throw new NotImplementedException();
        }
    }
}
