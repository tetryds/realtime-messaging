using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using tetryds.RealtimeMessaging.MemoryManagement;
using tetryds.RealtimeMessaging.Network.Exceptions;
using tetryds.RealtimeMessaging.Network.Internal;

namespace tetryds.RealtimeMessaging.Network
{
    public class SocketClientGateway<T> : IGateway<T>, IDisposable where T : IMessage, new()
    {
        readonly string host;
        readonly int port;

        MemoryPool memoryPool;

        SocketClient client;

        ConcurrentQueue<T> receivedMessages = new ConcurrentQueue<T>();

        bool disposed = false;

        public bool Connected => client?.Connected ?? false;

        public SocketClientGateway(int port, string host)
        {
            this.port = port;
            this.host = host;

            memoryPool = new MemoryPool();
        }

        public void Start()
        {
            Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            client = new SocketClient(socket, memoryPool);

            client.MessageRead += AddMessage;
            client.SocketShutdown += e => Disconnect();

            socket.Connect(host, port);
            client.Start();
        }

        public void Disconnect()
        {
            client.MessageRead -= AddMessage;
            client.Dispose();

            client = null;
        }

        public void Send(T message)
        {
            if (!Connected)
                throw new RemoteNotConnectedException();

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

        public bool DropSource(Guid id)
        {
            if (!Connected) return false;

            Disconnect();
            return true;
        }

        private void AddMessage(ReadBuffer readBuffer)
        {
            T message = new T();
            message.RemoteId = Guid.Empty;
            message.ReadFromBuffer(readBuffer);
            receivedMessages.Enqueue(message);
            readBuffer.Dispose();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            client?.Dispose();
            memoryPool.Dispose();
        }
    }
}
