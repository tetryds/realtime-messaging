using System.IO;
using tetryds.RealtimeMessaging.Buffer;

namespace tetryds.RealtimeMessaging
{
    public interface IMessage
    {
        int Id { get; set; }
        void Read(ReadBuffer reader);
        void Write(WriteBuffer writer);
    }
}