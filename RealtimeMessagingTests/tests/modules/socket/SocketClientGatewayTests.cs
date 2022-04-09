using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using tetryds.RealtimeMessaging.MemoryManagement;
using tetryds.RealtimeMessaging.Network;
using tetryds.RealtimeMessaging.Network.Internal;
using tetryds.Tests.Tools;

namespace tetryds.Tests.Standard
{
    [TestFixture]
    public class SocketClientGatewayTests
    {
        private const int Port = 888;

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
        public void SendReceiveSocketClientGateway()
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

            SocketClientGateway<SimpleMessage> clientGateway = new SocketClientGateway<SimpleMessage>(Port, "localhost");
            disposables.Add(clientGateway);

            clientGateway.Start();
            Assert.IsTrue(clientConnected.WaitOne(100));

            AutoResetEvent serverReceived = new AutoResetEvent(false);
            disposables.Add(serverReceived);
            ConcurrentQueue<ReadBuffer> serverQueue = new ConcurrentQueue<ReadBuffer>();

            Assert.NotNull(server);

            disposables.Add(server);
            server.MessageRead += r =>
            {
                serverReceived.Set();
                serverQueue.Enqueue(r);
            };

            SimpleMessage message = new SimpleMessage(LongMsg);
            clientGateway.Send(message);

            serverReceived.WaitOne(100);

            byte[] LongMsgData = Encoding.UTF8.GetBytes(LongMsg);

            Assert.True(serverQueue.TryDequeue(out ReadBuffer readBuffer));
            byte[] messageData = GetBytes(readBuffer);
            readBuffer.Dispose();
            string receivedMsg = Encoding.UTF8.GetString(messageData);

            Assert.AreEqual(LongMsg, receivedMsg);

            ReadBuffer readBuffer2 = GetReadBuffer(Encoding.UTF8.GetBytes(LongMsg), memoryPool);
            server.Send(readBuffer2);

            Assert.True(clientGateway.TryGet(100, out SimpleMessage message2));
            Assert.NotNull(message2);

            Assert.AreEqual(LongMsg, message2.Message);
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
