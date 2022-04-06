using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using tetryds.RealtimeMessaging.MemoryManagement;
using tetryds.RealtimeMessaging.Network.Internal;

namespace tetryds.RealtimeMessaging.Network
{
    public class SocketClientGateway<T> : IGateway<T>, IDisposable where T : IMessage, new()
    {
        readonly string host;
        readonly int port;
        readonly Socket socket;

        SocketClient client;
        MemoryPool memoryPool;

        ConcurrentQueue<T> receivedMessages = new ConcurrentQueue<T>();

        bool disposed = false;

        public SocketClientGateway(int port, string host)
        {
            this.port = port;
            this.host = host;

            memoryPool = new MemoryPool();

            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            client = new SocketClient(socket, memoryPool);

            client.MessageRead += AddMessage;
        }

        public void Connect()
        {
            socket.Connect(host, port);
            client.Start();
        }

        public bool ReleaseId(int id)
        {
            return true;
        }

        public void Send(T message)
        {
            MemoryStream memoryStream = memoryPool.Pop();
            message.WriteToBuffer(new WriteBuffer(memoryStream));
            memoryStream.Position = 0;
            ReadBuffer readBuffer = new ReadBuffer(memoryStream, memoryPool);
            client.Send(readBuffer);
            readBuffer.Dispose();
        }

        public bool TryGet(out T message)
        {
            return receivedMessages.TryDequeue(out message);
        }

        private void AddMessage(ReadBuffer readBuffer)
        {
            T message = new T();
            message.ReadFromBuffer(readBuffer);
            receivedMessages.Enqueue(message);
            readBuffer.Dispose();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            client.MessageRead -= AddMessage;
            client.Dispose();
            memoryPool.Dispose();
        }
    }
}
