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
    public class SimpleMessage : IMessage
    {
        public string Message { get; set; }

        public int Id { get; set; }

        public SimpleMessage()
        {
        }

        public SimpleMessage(string message)
        {
            Message = message;
        }

        public void ReadFromBuffer(ReadBuffer reader)
        {
            byte[] buffer = new byte[reader.Length];
            reader.Read(buffer, 0, buffer.Length);
            Message = Encoding.UTF8.GetString(buffer);
        }

        public void WriteToBuffer(WriteBuffer writer)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(Message);
            writer.Write(buffer, 0, buffer.Length);
        }
    }
}
