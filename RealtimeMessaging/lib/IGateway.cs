using System;

namespace tetryds.RealtimeMessaging
{
    public interface IGateway<T> where T : IMessage, new()
    {
        void Connect();
        void Send(T message);
        bool TryGet(out T message);
        bool ReleaseId(int id);
    }
}
