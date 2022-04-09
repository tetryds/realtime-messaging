using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using tetryds.RealtimeMessaging.MemoryManagement;
using tetryds.RealtimeMessaging.Network.Exceptions;

namespace tetryds.RealtimeMessaging.Network.Internal
{
    public class SocketClient : IDisposable
    {
        Socket socket;
        MemoryPool memoryPool;

        //Task listener;
        Thread listener;

        volatile bool running = false;
        volatile bool disposed = false;

        readonly byte[] lenBuffer = new byte[sizeof(int)];
        readonly byte[] readBuffer = new byte[4096];
        readonly byte[] sendBuffer = new byte[4096];

        public event Action<ReadBuffer> MessageRead;
        public event Action<Exception> SocketShutdown;

        public bool Connected => socket?.Connected ?? false;

        public SocketClient(Socket socket, MemoryPool memoryPool)
        {
            this.socket = socket;
            this.memoryPool = memoryPool;
        }

        public void Start()
        {
            EnsureState(nameof(Start), false);
            if (!socket.Connected)
                throw new Exception("Cannot start, socket not connected!");
            if (running) return;

            running = true;
            listener = new Thread(DoListen);
            listener.Priority = ThreadPriority.Highest;
            listener.IsBackground = true;
            listener.Start();
        }

        public void Send(ReadBuffer buffer)
        {
            EnsureState(nameof(Send), true);
            byte[] len = BitConverter.GetBytes((int)buffer.Length);
            int read;
            socket.Send(len);

            int left = (int)buffer.Length;
            int count = 0;
            while ((read = buffer.Read(sendBuffer)) > 0)
            {
                socket.Send(sendBuffer, read, SocketFlags.None);
                count++;
                count %= 500;
                if (count == 0)
                    Thread.Sleep(1);
            }
        }

        private void DoListen()
        {
            SocketError error = SocketError.Success;
            while (running)
            {
                try
                {
                    int count = socket.Receive(lenBuffer, 0, sizeof(int), SocketFlags.None, out error);
                    if (error != SocketError.Success)
                        throw new SocketConnectionException(error);
                    if (count != sizeof(int))
                        throw new SocketConnectionException(error, "Wrong package length data!");

                    int len = BitConverter.ToInt32(lenBuffer, 0);

                    if (len < 0)
                        throw new SocketConnectionException(error, $"Package length cannot be negative! Given len '{len}'");

                    int remaining = len;

                    MemoryStream memoryStream = memoryPool.Pop();
                    memoryStream.Capacity = len;
                    do
                    {
                        int toRead = readBuffer.Length > remaining ? remaining : readBuffer.Length;
                        int read = socket.Receive(readBuffer, 0, toRead, SocketFlags.None);
                        memoryStream.Write(readBuffer, 0, read);
                        remaining -= read;
                    }
                    while (remaining > 0);

                    memoryStream.Position = 0;
                    MessageRead?.Invoke(new ReadBuffer(memoryStream, memoryPool));

                }
                catch (Exception e)
                {
                    SocketShutdown?.Invoke(e);
                    Dispose();
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
            socket?.Disconnect(false);
            socket?.Dispose();
            listener.Join(2000);
            //listener?.Wait(2000);
            //listener?.Dispose();

            MessageRead = null;
            SocketShutdown = null;
        }
    }
}
