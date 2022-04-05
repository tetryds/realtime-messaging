using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NUnit.Framework;
using tetryds.RealtimeMessaging.MemoryManagement;
using tetryds.RealtimeMessaging.Network.Internal;

namespace tetryds.Tests.Standard
{
    [TestFixture]
    public class SocketTests
    {
        private const int Port = 999;

        private static readonly byte[] Data1 = new byte[] { 170, 187, 204, 221, 238, 255 };
        private static readonly byte[] Data2 = new byte[] { 43, 123, 111, 65, 210, 197, 234, 65, 12, 0, 99, 12, 152, 95, 219 };
        private static readonly string LongMsg = ")*&h0787qweh708&)*)*^qW56sad651asd43287#$*&as" +
            "dgasiudg74087234asiduhܳ2󨇽镾܂愹򧠪蟟Ӄ軤顏󲙨񽴶󳬨妫˫ڋѭ碵x󍻌ɀ鷇򅗚񋻍Ɇ~++Ĭ򥾽ॎ䈽ZE紖j<n걥Ԩ@򂠅㷑Ԭ^ހu" +
            "ȟ먓ஆS𢓏󠻢ķ𤛋Հ񭋲ܺ񋵪𞷌Գޟ㪷򂸧⺯Ε,񦅟Q３ؖ|ʫڙ݋ڧ鱵ۚ犍l̈́𩼋Pঙw񌍨ŘĀ)DҺ񍜚󃑤񖛟偘df皵~􀕍׍ǲnd񖇔󫷯.環m㪼񴔔ư񣣓Ⱊꤠ" +
            "떼E󨶟ϕ𝤇啓츣ř򧎪֯됕ى韬봒RɥԠ[隷ܤI剖ƫQ롣ں޼񙹠󝏨𩱠@󮅸߾򜈅oϮ";

        MemoryPool memoryPool;
        SocketServer socketServer;

        List<IDisposable> disposables = new List<IDisposable>();

        [SetUp]
        public void SetUp()
        {
            memoryPool = new MemoryPool();
            socketServer = new SocketServer(Port, memoryPool);

            disposables.Add(memoryPool);
            disposables.Add(socketServer);
        }

        [Test]
        public void SendDataSocketClients()
        {
            SocketClient server = null;
            AutoResetEvent clientConnected = new AutoResetEvent(false);

            socketServer.ClientConnected += c =>
            {
                clientConnected.Set();
                server = c;
            };

            socketServer.Start();

            Socket clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true
            };

            clientSocket.Connect("localhost", Port);
            clientSocket.NoDelay = true;
            SocketClient client = new SocketClient(clientSocket, memoryPool);
            client.Start();

            disposables.Add(client);

            Assert.IsTrue(clientConnected.WaitOne(100));

            Assert.NotNull(server);

            disposables.Add(server);

            AutoResetEvent serverReceived = new AutoResetEvent(false);
            AutoResetEvent clientReceived = new AutoResetEvent(false);

            disposables.Add(serverReceived);
            disposables.Add(clientReceived);

            ConcurrentQueue<ReadBuffer> serverQueue = new ConcurrentQueue<ReadBuffer>();
            ConcurrentQueue<ReadBuffer> clientQueue = new ConcurrentQueue<ReadBuffer>();

            server.MessageRead += r =>
            {
                serverReceived.Set();
                serverQueue.Enqueue(r);
            };

            client.MessageRead += r =>
            {
                clientReceived.Set();
                clientQueue.Enqueue(r);
            };

            ReadBuffer message1 = GetReadBuffer(Data1, memoryPool);
            ReadBuffer message2 = GetReadBuffer(Data2, memoryPool);
            byte[] LongMsgData = Encoding.UTF8.GetBytes(LongMsg);
            ReadBuffer message3 = GetReadBuffer(LongMsgData, memoryPool);

            ReadBuffer readBuffer;
            byte[] readData;

            client.Send(message1);
            Assert.IsTrue(serverReceived.WaitOne(100));
            Assert.IsTrue(serverQueue.TryDequeue(out readBuffer));
            readData = GetBytes(readBuffer);
            CollectionAssert.AreEqual(Data1, readData);

            server.Send(message2);
            Assert.IsTrue(clientReceived.WaitOne(100));
            Assert.IsTrue(clientQueue.TryDequeue(out readBuffer));
            readData = GetBytes(readBuffer);
            CollectionAssert.AreEqual(Data2, readData);

            client.Send(message3);
            Assert.IsTrue(serverReceived.WaitOne(100));
            Assert.IsTrue(serverQueue.TryDequeue(out readBuffer));
            readData = GetBytes(readBuffer);
            CollectionAssert.AreEqual(LongMsgData, readData);
        }

        [Test]
        public void SendDataMockSocket()
        {

            SocketClient client = null;
            AutoResetEvent clientConnected = new AutoResetEvent(false);

            socketServer.ClientConnected += c =>
            {
                clientConnected.Set();
                client = c;
            };

            socketServer.Start();

            Socket clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true
            };

            clientSocket.Connect("localhost", Port);

            disposables.Add(clientSocket);

            Assert.IsTrue(clientConnected.WaitOne(100));

            Assert.NotNull(client);

            ReadBuffer readBuffer = null;
            AutoResetEvent msgReceived = new AutoResetEvent(false);

            client.MessageRead += r =>
            {
                msgReceived.Set();
                readBuffer = r;
            };

            byte[] len = BitConverter.GetBytes(Data1.Length);
            clientSocket.Send(len);
            clientSocket.Send(Data1);

            Assert.IsTrue(msgReceived.WaitOne(100));

            Assert.NotNull(readBuffer);

            byte[] readData = new byte[readBuffer.Length];
            readBuffer.Read(readData);

            //Console.WriteLine($"Returned data: {string.Join("-", readData)}");

            CollectionAssert.AreEqual(Data1, readData);
        }

        [TearDown]
        public void Teardown()
        {
            disposables.ForEach(d => d.Dispose());
        }

        private ReadBuffer GetReadBuffer(byte[] data, MemoryPool memoryPool)
        {
            MemoryStream memoryStream = memoryPool.Pop();
            memoryStream.Write(data, 0, data.Length);
            memoryStream.Position = 0;
            return new ReadBuffer(memoryStream, memoryPool);
        }

        private byte[] GetBytes(ReadBuffer readBuffer)
        {
            byte[] data = new byte[readBuffer.Length];
            readBuffer.Read(data);
            return data;
        }
    }
}
