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
    public class SocketServerGatewayTests
    {
        private const int Port = 999;
        private const int MessageSizeSmall = 4096;
        private const int MessageSizeHuge = 262144;
        private const int RanodmSeed = 333;
        private const int TimeoutMs = 100;
        private const int SleepMs = 5;
        private const string Localhost = "localhost";
        List<IDisposable> disposables = new List<IDisposable>();

        Random random;

        [SetUp]
        public void SetUp()
        {
            random = new Random(RanodmSeed);
        }

        [Test]
        public void SendReceive()
        {
            SocketServerGateway<ByteArrayMessage> server = new SocketServerGateway<ByteArrayMessage>(Port);
            disposables.Add(server);
            SocketClientGateway<ByteArrayMessage> client1 = new SocketClientGateway<ByteArrayMessage>(Port, Localhost);
            disposables.Add(client1);

            server.Start();
            client1.Start();

            ByteArrayMessage request1 = new ByteArrayMessage(GetRandomBytes(MessageSizeHuge));

            client1.Send(request1);

            Assert.IsTrue(TryGetTimeout(server, TimeoutMs, out ByteArrayMessage arrived1));
            CollectionAssert.AreEqual(request1.Data, arrived1.Data);

            Guid client1Id = arrived1.RemoteId;

            ByteArrayMessage request2 = new ByteArrayMessage(GetRandomBytes(MessageSizeHuge), client1Id);
            server.Send(request2);

            Assert.IsTrue(TryGetTimeout(client1, TimeoutMs, out ByteArrayMessage arrived2));
            CollectionAssert.AreEqual(request2.Data, arrived2.Data);
        }

        [Test]
        public void SendReceiveMultipleClients()
        {
            SocketServerGateway<ByteArrayMessage> server = new SocketServerGateway<ByteArrayMessage>(Port);
            disposables.Add(server);
            SocketClientGateway<ByteArrayMessage> client1 = new SocketClientGateway<ByteArrayMessage>(Port, Localhost);
            disposables.Add(client1);
            SocketClientGateway<ByteArrayMessage> client2 = new SocketClientGateway<ByteArrayMessage>(Port, Localhost);
            disposables.Add(client2);

            server.Start();

            client1.Start();

            ByteArrayMessage request1 = new ByteArrayMessage(GetRandomBytes(MessageSizeHuge));

            client1.Send(request1);

            Assert.IsTrue(TryGetTimeout(server, TimeoutMs, out ByteArrayMessage arrived1));
            CollectionAssert.AreEqual(request1.Data, arrived1.Data);

            Guid client1Id = arrived1.RemoteId;

            ByteArrayMessage request2 = new ByteArrayMessage(GetRandomBytes(MessageSizeHuge), client1Id);
            server.Send(request2);

            Assert.IsTrue(TryGetTimeout(client1, TimeoutMs, out ByteArrayMessage arrived2));
            CollectionAssert.AreEqual(request2.Data, arrived2.Data);

            client2.Start();

            ByteArrayMessage request3 = new ByteArrayMessage(GetRandomBytes(MessageSizeHuge));
            client2.Send(request3);

            Assert.IsTrue(TryGetTimeout(server, TimeoutMs, out ByteArrayMessage arrived3));
            CollectionAssert.AreEqual(request3.Data, arrived3.Data);

            Guid client2Id = arrived3.RemoteId;

            ByteArrayMessage request4 = new ByteArrayMessage(GetRandomBytes(MessageSizeHuge), client2Id);
            server.Send(request4);

            Assert.IsTrue(TryGetTimeout(client2, TimeoutMs, out ByteArrayMessage arrived4));
            CollectionAssert.AreEqual(request4.Data, arrived4.Data);

            Assert.AreEqual(2, server.SourceCount);
        }

        [Test]
        public void DropSource()
        {
            SocketServerGateway<ByteArrayMessage> server = new SocketServerGateway<ByteArrayMessage>(Port);
            disposables.Add(server);
            SocketClientGateway<ByteArrayMessage> client1 = new SocketClientGateway<ByteArrayMessage>(Port, Localhost);
            disposables.Add(client1);

            server.Start();
            client1.Start();

            Task timeoutTask = Task.Delay(TimeoutMs);
            while (server.SourceCount == 0 && !timeoutTask.IsCompleted) { Thread.Sleep(SleepMs); }
            Assert.IsFalse(timeoutTask.IsCompleted);

            Guid clientId = server.GetSourceIds().First();

            Assert.True(server.DropSource(clientId));

            ByteArrayMessage request1 = new ByteArrayMessage(GetRandomBytes(MessageSizeSmall));
            request1.RemoteId = clientId;

            Assert.Throws<RemoteNotConnectedException>(() => server.Send(request1));
        }

        [Test]
        public void DropInexistentSource()
        {
            SocketServerGateway<ByteArrayMessage> server = new SocketServerGateway<ByteArrayMessage>(Port);
            disposables.Add(server);
            server.Start();

            Assert.False(server.DropSource(Guid.NewGuid()));
        }

        [Test]
        public void RejectWrongGuid()
        {
            SocketServerGateway<ByteArrayMessage> server = new SocketServerGateway<ByteArrayMessage>(Port);
            disposables.Add(server);

            server.Start();

            ByteArrayMessage request1 = new ByteArrayMessage(GetRandomBytes(MessageSizeSmall));
            request1.RemoteId = Guid.NewGuid();

            Assert.Throws<RemoteNotConnectedException>(() => server.Send(request1));
        }

        [TearDown]
        public void Teardown()
        {
            disposables.ForEach(d => d.Dispose());
        }

        private bool TryGetTimeout<T>(IGateway<T> gateway, int timeoutMs, out T msg) where T : IMessage, new()
        {
            Task timeoutTask = Task.Delay(timeoutMs);
            while (!gateway.TryGet(out msg) && !timeoutTask.IsCompleted) { Thread.Sleep(SleepMs); }
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
