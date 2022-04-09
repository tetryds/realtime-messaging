using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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

        volatile bool disposed = false;
        volatile bool running = false;

        public bool Connected => client?.Connected ?? false;

        public SocketClientGateway(int port, string host) : this(port, host, new MemoryPool()) { }

        public SocketClientGateway(int port, string host, MemoryPool memoryPool)
        {
            this.port = port;
            this.host = host;

            this.memoryPool = memoryPool;
        }

        public void Start()
        {
            EnsureState(nameof(Start), false, false);
            running = true;

            Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            client = new SocketClient(socket, memoryPool);

            client.MessageRead += AddMessage;
            client.SocketShutdown += e => Disconnect();

            socket.Connect(host, port);
            client.Start();
        }

        public void Disconnect()
        {
            client.Dispose();

            client = null;
        }

        public void Send(T message)
        {
            EnsureState(nameof(Send), true, true);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureState(string operation, bool shouldBeRunning, bool shouldBeConnected)
        {
            if (disposed)
                throw new ObjectDisposedException($"Cannot execute '{operation}', socket client has been disposed and cannot be reused");
            if (running != shouldBeRunning)
                throw new InvalidOperationException($"Cannot execute '{operation}', socket client is {(shouldBeRunning ? "not " : "")}running");
            if (Connected != shouldBeConnected)
                throw new InvalidOperationException($"Cannot execute '{operation}', socket is {(shouldBeConnected ? "not " : "")}connected");
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            running = false;
            client?.Dispose();
            memoryPool.Dispose();
        }
    }
}
