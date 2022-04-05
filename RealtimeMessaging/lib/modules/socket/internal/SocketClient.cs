using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using tetryds.RealtimeMessaging.MemoryManagement;

namespace tetryds.RealtimeMessaging.Network.Internal
{
    public class SocketClient : IDisposable
    {
        const int LEN_BUFFER_SIZE = 4;

        Socket socket;
        MemoryPool memoryPool;

        Task listener;

        volatile bool running = false;

        readonly byte[] lenBuffer = new byte[LEN_BUFFER_SIZE];
        readonly byte[] readBuffer = new byte[4096];
        readonly byte[] sendBuffer = new byte[4096];

        public event Action<ReadBuffer> MessageRead;
        public event Action<Exception> ErrorOcurred;
        public event Action SocketShutdown;

        public SocketClient(Socket socket, MemoryPool memoryPool)
        {
            this.socket = socket;
            this.memoryPool = memoryPool;
        }

        public void Start()
        {
            if (running) return;

            running = true;
            listener = new Task(DoListen, TaskCreationOptions.LongRunning);
            listener.Start();
        }

        public void Send(ReadBuffer buffer)
        {
            byte[] len = BitConverter.GetBytes((int)buffer.Length);
            Console.WriteLine($"Sending message of '{(int)buffer.Length}'");
            socket.Send(len);
            int read = 0;
            while ((read = buffer.Read(sendBuffer)) > 0)
            {
                Console.WriteLine($"Send buffer: '{string.Join("-", sendBuffer)}'");
                socket.Send(sendBuffer, read, SocketFlags.None);
            }
        }

        private void DoListen()
        {
            while (running)
            {
                try
                {
                    Console.WriteLine($"Ready to receive message size");
                    int count = socket.Receive(lenBuffer, LEN_BUFFER_SIZE, SocketFlags.None);
                    if (count != LEN_BUFFER_SIZE)
                        throw new Exception("Wrong package length data!");

                    int len = BitConverter.ToInt32(lenBuffer, 0);

                    int remaining = len;

                    MemoryStream memoryStream = memoryPool.Pop();
                    Console.WriteLine($"Message of size '{len}' being received");
                    do
                    {
                        int toRead = readBuffer.Length > remaining ? remaining : readBuffer.Length;
                        Console.WriteLine($"Reading '{toRead}' bytes, '{remaining}' remaining");
                        int read = socket.Receive(readBuffer, 0, toRead, SocketFlags.None);
                        Console.WriteLine($"Read buffer: '{string.Join("-", readBuffer)}'");
                        memoryStream.Write(readBuffer, 0, read);
                        remaining -= read;
                    }
                    while (remaining > 0);

                    Console.WriteLine($"Message read");

                    memoryStream.Position = 0;
                    MessageRead?.Invoke(new ReadBuffer(memoryStream, memoryPool));

                }
                catch (Exception e)
                {
                    if (socket.Connected)
                    {
                        Console.WriteLine($"Something went wrong {e}");
                        ErrorOcurred?.Invoke(e);
                    }
                    else
                    {
                        Console.WriteLine($"Socket Shutdown");
                        SocketShutdown?.Invoke();
                    }
                }
            }
        }

        public void Dispose()
        {
            running = false;
            socket.Disconnect(false);
            socket.Dispose();
            listener.Wait(2000);
            listener.Dispose();
        }
    }
}
