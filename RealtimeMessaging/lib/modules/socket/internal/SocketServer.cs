using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace tetryds.RealtimeMessaging.Network.Internal
{
    public class SocketServer
    {
        const int MAX_MSG_SIZE = 65536;
        int port;
        TcpListener listener;

        bool running = false;

        Dictionary<Guid, TcpClient> clientMap;


        public SocketServer(int port)
        {
            this.port = port;
        }

        public void StartListener()
        {
            if (running)
                throw new InvalidOperationException("Socket server already running!");
            running = true;

            IPAddress address = IPAddress.Any;
            listener = new TcpListener(address, port);
            listener.Start();
        }

        private void DoAcceptClient()
        {
            while (running)
            {
                try
                {
                    Socket client = listener.AcceptSocket();
                    Guid guid = Guid.NewGuid();
                    ServerListener serverListener = new ServerListener(guid, client);
                }
                catch
                {
                    // TODO: log
                }
            }
        }

        private class ServerListener
        {
            Guid guid;
            Socket socket;

            bool running = false;

            byte[] lenBuffer = new byte[4];
            byte[] readBuffer = new byte[0];

            Action<Exception> ErrorOcurred;

            public ServerListener(Guid guid, Socket socket)
            {
                this.guid = guid;
                this.socket = socket;
            }

            public void StartListener()
            {
                Task.Run(DoListen, )
            }

            private void DoListen()
            {
                while (running)
                {
                    try
                    {
                        int count = socket.Receive(lenBuffer, 4, SocketFlags.None);
                        if (count != 4)
                            throw new Exception("Wrong package length data!");
                        int len = BitConverter.ToInt32(lenBuffer, 0);
                        int readLen = socket.Receive()
                    }
                    catch(Exception e)
                    {
                        ErrorOcurred?.Invoke(e);
                        running = false;
                    }
                }
            }

            private void AdjustBufferSize(int size)
            {
                if (size > MAX_MSG_SIZE)
                    throw new Exception($"Message size of '{size}' is too big! Max message size is '{MAX_MSG_SIZE}'")
                if (readBuffer.Length >= size) return;

                Array.Resize(ref readBuffer, size);

            }
        }
    }
}
