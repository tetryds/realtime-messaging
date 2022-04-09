using System;

namespace tetryds.RealtimeMessaging
{
    public interface IGateway<T> where T : IMessage, new()
    {
        void Start();
        void Send(T message);
        bool TryGet(int millisecondsTimeout, out T message);
        bool DropSource(Guid id);
    }
}
