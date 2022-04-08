using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using tetryds.RealtimeMessaging.MemoryManagement;
using tetryds.RealtimeMessaging.Network.Exceptions;
using tetryds.RealtimeMessaging.Network.Internal;

namespace tetryds.RealtimeMessaging.Network
{
    public class SocketServerGateway<T> : IGateway<T>, IDisposable where T : IMessage, new()
    {
        readonly int port;
        readonly MemoryPool memoryPool;

        readonly SocketServer socketServer;

        readonly ConcurrentQueue<T> receivedMessages;
        readonly ConcurrentDictionary<Guid, SocketClient> clientMap;

        public event Action<Exception> ErrorOccurred;

        public SocketServerGateway(int port)
        {
            this.port = port;

            receivedMessages = new ConcurrentQueue<T>();
            clientMap = new ConcurrentDictionary<Guid, SocketClient>();

            memoryPool = new MemoryPool();
            socketServer = new SocketServer(port, memoryPool);

            socketServer.ClientConnected += RegisterClient;
            socketServer.ErrorOcurred += ErrorOccurred;
        }

        public void Start()
        {
            socketServer.Start();
        }

        public void Send(T message)
        {
            if (!clientMap.TryGetValue(message.RemoteId, out SocketClient client))
                throw new RemoteNotConnectedException(message.RemoteId);

            SendToClient(message, client);
        }

        public bool TryGet(out T message)
        {
            return receivedMessages.TryDequeue(out message);
        }


        public bool DropSource(Guid clientId)
        {
            if (!clientMap.TryRemove(clientId, out SocketClient client)) return false;

            client.Dispose();
            return true;
        }

        public List<Guid> GetSourceIds() => clientMap.Keys.ToList();
        public int SourceCount => clientMap.Count;

        private void RegisterClient(SocketClient client)
        {
            Guid clientId = Guid.NewGuid();
            client.MessageRead += m => AddMessage(clientId, m);
            client.SocketShutdown += e => DropSource(clientId);
            clientMap.TryAdd(clientId, client);
        }

        private void SendToClient(T message, SocketClient client)
        {
            MemoryStream memoryStream = memoryPool.Pop();
            message.WriteToBuffer(new WriteBuffer(memoryStream));
            memoryStream.Position = 0;
            ReadBuffer readBuffer = new ReadBuffer(memoryStream, memoryPool);
            client.Send(readBuffer);
            readBuffer.Dispose();
        }

        private void AddMessage(Guid clientId, ReadBuffer readBuffer)
        {
            T message = new T();
            message.RemoteId = clientId;
            message.ReadFromBuffer(readBuffer);
            receivedMessages.Enqueue(message);
            readBuffer.Dispose();
        }

        public void Dispose()
        {
            socketServer.Dispose();
            foreach (Guid clientId in clientMap.Keys)
            {
                clientMap[clientId].Dispose();
            }
            memoryPool.Dispose();
        }
    }
}
