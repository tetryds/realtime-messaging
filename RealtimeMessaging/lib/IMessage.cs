﻿using System.IO;
using tetryds.RealtimeMessaging.MemoryManagement;

namespace tetryds.RealtimeMessaging
{
    public interface IMessage
    {
        int Id { get; set; }
        void Read(ReadBuffer reader);
        void Write(WriteBuffer writer);
    }
}