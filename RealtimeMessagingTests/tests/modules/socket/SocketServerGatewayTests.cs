using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using tetryds.RealtimeMessaging;
using tetryds.RealtimeMessaging.MemoryManagement;
using tetryds.RealtimeMessaging.Network;
using tetryds.RealtimeMessaging.Network.Internal;
using tetryds.Tests.Tools;

namespace tetryds.Tests.Standard
{
    [TestFixture]
    public class SocketServerGatewayTests
    {
        private const int Port = 999;
        private const int MessageSize = 262144;
        private const int RanodmSeed = 333;

        List<IDisposable> disposables = new List<IDisposable>();

        Random random;

        [SetUp]
        public void SetUp()
        {
            random = new Random(RanodmSeed);
        }

        [Test]
        public void SendReceiveSocketServerGateway()
        {
            SocketServerGateway<ByteArrayMessage> server = new SocketServerGateway<ByteArrayMessage>(Port);
            disposables.Add(server);
            SocketClientGateway<ByteArrayMessage> client1 = new SocketClientGateway<ByteArrayMessage>(Port, "localhost");
            disposables.Add(client1);
            SocketClientGateway<ByteArrayMessage> client2 = new SocketClientGateway<ByteArrayMessage>(Port, "localhost");
            disposables.Add(client2);

            server.Connect();

            client1.Connect();

            ByteArrayMessage request1 = new ByteArrayMessage(GetRandomBytes(MessageSize));

            client1.Send(request1);

            Assert.IsTrue(TryGetTimeout(server, 100, out ByteArrayMessage arrived1));
            CollectionAssert.AreEqual(request1.Data, arrived1.Data);

            Guid client1Id = arrived1.SourceId;

            ByteArrayMessage request2 = new ByteArrayMessage(GetRandomBytes(MessageSize), client1Id);
            server.Send(request2);

            Assert.IsTrue(TryGetTimeout(client1, 100, out ByteArrayMessage arrived2));
            CollectionAssert.AreEqual(request2.Data, arrived2.Data);

            client2.Connect();

            ByteArrayMessage request3 = new ByteArrayMessage(GetRandomBytes(MessageSize));
            client2.Send(request3);

            Assert.IsTrue(TryGetTimeout(server, 100, out ByteArrayMessage arrived3));
            CollectionAssert.AreEqual(request3.Data, arrived3.Data);

            Guid client2Id = arrived3.SourceId;

            ByteArrayMessage request4 = new ByteArrayMessage(GetRandomBytes(MessageSize), client2Id);
            server.Send(request4);

            Assert.IsTrue(TryGetTimeout(client2, 100, out ByteArrayMessage arrived4));
            CollectionAssert.AreEqual(request4.Data, arrived4.Data);
        }

        [TearDown]
        public void Teardown()
        {
            disposables.ForEach(d => d.Dispose());
        }

        private bool TryGetTimeout<T>(IGateway<T> gateway, int timeoutMs, out T msg) where T : IMessage, new()
        {
            Task timeoutTask = Task.Delay(timeoutMs);
            while (!gateway.TryGet(out msg) && !timeoutTask.IsCompleted) { }
            return !timeoutTask.IsCompleted;
        }

        private byte[] GetRandomBytes(int size)
        {
            byte[] data = new byte[size];
            random.NextBytes(data);
            return data;
        }
    }
}
