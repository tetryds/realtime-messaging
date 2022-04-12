using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using tetryds.RealtimeMessaging;
using tetryds.RealtimeMessaging.MemoryManagement;
using tetryds.RealtimeMessaging.Network;
using tetryds.RealtimeMessaging.Network.Exceptions;
using tetryds.RealtimeMessaging.Network.Internal;
using tetryds.Tests.Tools;

namespace tetryds.Tests.Standard
{
    [TestFixture]
    public class SocketFlowTests
    {
        private const int Port = 999;
        private const int MessageRate = 2;
        private const int MessageSize = 4096;
        private const int MessageCount = 100_000;

        private const int RanodmSeed = 333;
        private const int TimeoutMs = 100;

        private const int SleepMs = 5;
        private const string Localhost = "localhost";
        List<IDisposable> disposables = new List<IDisposable>();

        Random random;

        byte[] data;

        [SetUp]
        public void SetUp()
        {
            random = new Random(RanodmSeed);
            data = new byte[MessageSize];
        }

        [Test]
        public void SendReceiveMultipleMessages()
        {
            Exception exception = null;
            SocketServerGateway<ByteArrayMessage> server = new SocketServerGateway<ByteArrayMessage>(Port);
            disposables.Add(server);
            server.ErrorOccurred += e => exception = e;

            SocketClientGateway<ByteArrayMessage> client = new SocketClientGateway<ByteArrayMessage>(Port, Localhost);
            disposables.Add(client);

            server.Start();
            client.Start();

            Console.WriteLine("Starting to send");

            Task clientTask = Task.Run(() =>
            {
                try
                {
                    for (int i = 0; i < MessageCount; i++)
                    {
                        if (exception != null) break;
                        ByteArrayMessage request = new ByteArrayMessage(data);
                        client.Send(request);
                    }
                }
                catch (Exception e)
                {
                    exception = e;
                    throw e;
                }

                Console.WriteLine("Client done!");
            });

            Task serverTask = Task.Run(() =>
            {
                try
                {
                    int read = 0;
                    while (read < MessageCount)
                    {
                        if (exception != null) break;
                        if (server.TryGet(TimeoutMs, out _))
                            read++;
                    }
                }
                catch (Exception e)
                {
                    exception = e;
                    throw e;
                }

                Console.WriteLine("Server done!");
            });

            Console.WriteLine(exception);

            Task.WaitAny(clientTask, serverTask);
            Task.WaitAll(clientTask, serverTask);
        }

        [TearDown]
        public void Teardown()
        {
            disposables.ForEach(d => d.Dispose());
        }

        private byte[] GetRandomBytes(int size)
        {
            byte[] data = new byte[size];
            random.NextBytes(data);
            return data;
        }
    }
}
