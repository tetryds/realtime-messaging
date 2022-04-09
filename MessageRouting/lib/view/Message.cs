using System;
using tetryds.RealtimeMessaging.MemoryManagement;

namespace tetryds.RealtimeMessaging
{
    public class Message : IMessage
    {
        public Guid RemoteId { get; set; }

        public char Type;
        public byte[] Data;

        public void ReadFromBuffer(ReadBuffer reader)
        {
            if (reader.Length < 2)
                throw new Exception("Malformed message, messages must contain at least two bytes for type");
            
            byte[] typeBytes = new byte[2];
            reader.Read(typeBytes, 0, 2);

            Type = BitConverter.ToChar(typeBytes, 0);
            Data = new byte[reader.Remaining];
            reader.Read(Data);
        }

        public void WriteToBuffer(WriteBuffer writer)
        {
            writer.Write(BitConverter.GetBytes(Type));
            writer.Write(Data);
        }
    }
}
