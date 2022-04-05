using System.IO;
using tetryds.RealtimeMessaging.MemoryManagement;

namespace tetryds.RealtimeMessaging
{
    public interface IMessage
    {
        int Id { get; set; }
        void ReadFromBuffer(ReadBuffer reader);
        void WriteToBuffer(WriteBuffer writer);
    }
}