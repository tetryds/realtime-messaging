﻿using System;
using tetryds.RealtimeMessaging.MemoryManagement;

namespace tetryds.RealtimeMessaging
{
    public class Message : IMessage
    {
        private const int HEADER_SIZE = sizeof(char) + sizeof(Status);

        public Guid RemoteId { get; set; }

        public char Type;
        public Status Status;
        public byte[] Data;

        public void ReadFromBuffer(ReadBuffer reader)
        {
            if (reader.Length < HEADER_SIZE)
                throw new BadMessageException(typeof(Message), "Wrong message length");

            byte[] header = new byte[HEADER_SIZE];
            reader.Read(header);
            Type = BitConverter.ToChar(header, 0);
            Status = (Status)BitConverter.ToUInt16(header, sizeof(char));

            Data = new byte[reader.Remaining];
            reader.Read(Data);
        }

        public void WriteToBuffer(WriteBuffer writer)
        {
            writer.Write(BitConverter.GetBytes(Type));
            writer.Write(BitConverter.GetBytes((ushort)Status));
            writer.Write(Data);
        }
    }
}
