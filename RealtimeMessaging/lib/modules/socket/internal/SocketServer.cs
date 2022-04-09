using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using tetryds.RealtimeMessaging.MemoryManagement;

namespace tetryds.RealtimeMessaging.Network.Internal
{
    public class SocketServer : IDisposable
    {
        readonly int port;
        readonly MemoryPool memoryPool;

        TcpListener listener;
        Thread acceptor;

        volatile bool running = false;
        volatile bool disposed = false;

        public event Action<SocketClient> ClientConnected;
        public event Action<Exception> ErrorOcurred;

        public SocketServer(int port, MemoryPool memoryPool)
        {
            this.port = port;
            this.memoryPool = memoryPool;
        }

        public void Start()
        {
            EnsureState(nameof(Start), false);
            running = true;

            IPAddress address = IPAddress.Any;
            listener = new TcpListener(address, port);
            listener.Start();

            acceptor = new Thread(DoAcceptClient);
            acceptor.IsBackground = true;
            acceptor.Priority = ThreadPriority.BelowNormal;
            acceptor.Start();
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureState(string operation, bool shouldBeRunning)
        {
            if (disposed)
                throw new ObjectDisposedException($"Cannot execute '{operation}', socket client has been disposed and cannot be reused");
            if (running != shouldBeRunning)
                throw new InvalidOperationException($"Cannot execute '{operation}', socket client is {(shouldBeRunning ? "not " : "")}running");
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            running = false;
            listener?.Stop();
            acceptor.Join(2000);

            ClientConnected = null;
            ErrorOcurred = null;
        }
    }
}
