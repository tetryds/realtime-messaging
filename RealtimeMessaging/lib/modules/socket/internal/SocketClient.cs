using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using tetryds.RealtimeMessaging.MemoryManagement;
using tetryds.RealtimeMessaging.Network.Errors;

namespace tetryds.RealtimeMessaging.Network.Internal
{
    public class SocketClient : IDisposable
    {
        Socket socket;
        MemoryPool memoryPool;

        Task listener;

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
            EnsureNotDisposed();
            if (!socket.Connected)
                throw new Exception("Cannot start, socket not connected!");
            if (running) return;

            running = true;
            listener = new Task(DoListen, TaskCreationOptions.LongRunning);
            listener.Start();
        }

        public void Send(ReadBuffer buffer)
        {
            EnsureNotDisposed();
            byte[] len = BitConverter.GetBytes((int)buffer.Length);
            //Console.WriteLine($"Sending message of '{(int)buffer.Length}'");
            socket.Send(len);
            int read = 0;
            while ((read = buffer.Read(sendBuffer)) > 0)
            {
                //Console.WriteLine($"Send buffer: '{string.Join("-", sendBuffer)}'");
                socket.Send(sendBuffer, read, SocketFlags.None);
            }
        }

        private void DoListen()
        {
            SocketError error = SocketError.Success;
            while (running)
            {
                try
                {
                    //Console.WriteLine($"Ready to receive message size");
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
                    //Console.WriteLine($"Message of size '{len}' being received");
                    do
                    {
                        int toRead = readBuffer.Length > remaining ? remaining : readBuffer.Length;
                        //Console.WriteLine($"Reading '{toRead}' bytes, '{remaining}' remaining");
                        int read = socket.Receive(readBuffer, 0, toRead, SocketFlags.None);
                        //Console.WriteLine($"Read buffer: '{string.Join("-", readBuffer)}'");
                        memoryStream.Write(readBuffer, 0, read);
                        remaining -= read;
                    }
                    while (remaining > 0);

                    //Console.WriteLine($"Message read");

                    memoryStream.Position = 0;
                    MessageRead?.Invoke(new ReadBuffer(memoryStream, memoryPool));

                }
                catch (Exception e)
                {
                    //Console.WriteLine($"Socket Shutdown");
                    SocketShutdown?.Invoke(e);
                    Dispose();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureNotDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException("Socket client has been disposed and cannot be reused");
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            running = false;
            socket.Disconnect(false);
            socket.Dispose();
            listener.Wait(2000);
            listener.Dispose();
        }
    }
}
