using System;

namespace tetryds.RealtimeMessaging.Network
{
    public class MessageManager<T> where T : IMessage, new()
    {
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
