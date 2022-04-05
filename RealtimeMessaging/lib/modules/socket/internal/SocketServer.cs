using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using tetryds.RealtimeMessaging.MemoryManagement;

namespace tetryds.RealtimeMessaging.Network.Internal
{
    public class SocketServer : IDisposable
    {
        const int MAX_MSG_SIZE = 65536;

        readonly int port;
        readonly MemoryPool memoryPool;

        Task clientAcceptor;

        TcpListener listener;

        volatile bool running = false;

        public event Action<SocketClient> ClientConnected;
        public event Action<Exception> ErrorOcurred;

        public SocketServer(int port, MemoryPool memoryPool)
        {
            this.port = port;
            this.memoryPool = memoryPool;
        }

        public void Start()
        {
            if (running)
                throw new InvalidOperationException("Socket server already running!");
            running = true;

            IPAddress address = IPAddress.Any;
            listener = new TcpListener(address, port);
            listener.Start();

            clientAcceptor = new Task(DoAcceptClient, TaskCreationOptions.LongRunning);
            clientAcceptor.Start();
        }

        private void DoAcceptClient()
        {
            while (running)
            {
                try
                {
                    Socket client = listener.AcceptSocket();
                    SocketClient socketClient = new SocketClient(client, memoryPool);
                    socketClient.Start();
                    ClientConnected?.Invoke(socketClient);
                }
                catch (Exception e)
                {
                    ErrorOcurred?.Invoke(e);
                }
            }
        }

        public void Dispose()
        {
            running = false;
            listener.Stop();
            clientAcceptor.Wait(2000);
            clientAcceptor.Dispose();
        }
    }
}
