using System;
using tetryds.RealtimeMessaging.MemoryManagement;

namespace tetryds.RealtimeMessaging
{
    public interface IMessage
    {
        Guid RemoteId { get; set; }
        void ReadFromBuffer(ReadBuffer reader);
        void WriteToBuffer(WriteBuffer writer);
    }
}