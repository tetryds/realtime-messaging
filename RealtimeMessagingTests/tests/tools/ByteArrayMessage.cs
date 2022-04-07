using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NUnit.Framework;
using tetryds.RealtimeMessaging;
using tetryds.RealtimeMessaging.MemoryManagement;
using tetryds.RealtimeMessaging.Network.Internal;

namespace tetryds.Tests.Tools
{
    public class ByteArrayMessage : IMessage
    {
        public byte[] Data;

        public Guid SourceId { get; set; } = Guid.Empty;

        public ByteArrayMessage() { }

        public ByteArrayMessage(byte[] data)
        {
            Data = data;
        }

        public ByteArrayMessage(byte[] data, Guid sourceId)
        {
            Data = data;
            SourceId = sourceId;
        }

        public void ReadFromBuffer(ReadBuffer reader)
        {
            Data = new byte[reader.Length];
            reader.Read(Data, 0, Data.Length);
        }

        public void WriteToBuffer(WriteBuffer writer)
        {
            writer.Write(Data, 0, Data.Length);
        }
    }
}
