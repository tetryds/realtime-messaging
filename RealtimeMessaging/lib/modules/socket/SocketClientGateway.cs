using System;
using System.IO;
using System.Net.Sockets;
using tetryds.RealtimeMessaging.MemoryManagement;
using tetryds.RealtimeMessaging.Network.Internal;

namespace tetryds.RealtimeMessaging.Network
{
    public class SocketClientGateway<T> : IGateway<T>, IDisposable where T : IMessage, new()
    {
        string host;
        int port;
        Socket socket;

        SocketClient client;
        MemoryPool memoryPool;


        public SocketClientGateway(int port, string host)
        {
            this.port = port;
            this.host = host;

            memoryPool = new MemoryPool();

            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            client = new SocketClient(socket, memoryPool);
        }

        public void Connect()
        {
            socket.Connect(host, port);
            client.Start();
        }

        public bool ReleaseId(int id)
        {
            throw new NotImplementedException();
        }

        public void Send(T message)
        {
            MemoryStream memoryStream = memoryPool.Pop();
            message.WriteToBuffer(new WriteBuffer(memoryStream));
            memoryStream.Position = 0;
            client.Send(new ReadBuffer(memoryStream, memoryPool));
        }

        public bool TryGet(out T message)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            client.Dispose();
            memoryPool.Dispose();
        }
    }
}
