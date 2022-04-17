using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using tetryds.RealtimeMessaging.MemoryManagement;
using tetryds.RealtimeMessaging.Network.Exceptions;

namespace tetryds.RealtimeMessaging.Consumers
{
    public class ExceptionNotifierConsumer : IMessageConsumer
    {
        public event Action<Message> Replied;

        HashSet<Guid> errorConsumers = new HashSet<Guid>();

        public void NotifyError(Exception exception)
        {
            lock (errorConsumers)
            {
                if (exception is RemoteNotConnectedException remoteNotConnected)
                    errorConsumers.Remove(remoteNotConnected.ClientId);

                //Send message with error metadata
                foreach (Guid guid in errorConsumers)
                {
                    Replied?.Invoke(GetMessage(exception, guid));
                }
            }
        }

        public void Consume(Message message)
        {
            if (message.Data.Length != sizeof(bool))
                throw new BadMessageException(typeof(ExceptionNotifierConsumer), $"Wrong message payload length: '{message.Data.Length}'. Expected length: '1'");
            bool register = BitConverter.ToBoolean(message.Data, 0);
            bool success = register ? Register(message.RemoteId) : Unregister(message.RemoteId);
        }

        private bool Register(Guid guid)
        {
            lock (errorConsumers)
            {
                return errorConsumers.Add(guid);
            }
        }

        private bool Unregister(Guid clientId)
        {
            lock (errorConsumers)
            {
                return errorConsumers.Remove(clientId);
            }
        }

        private Message GetMessage(Exception e, Guid cliendId)
        {
            Message message = new Message();
            message.RemoteId = cliendId;
            message.Status = Status.Continue;
            message.Data = Encoding.UTF8.GetBytes(e.ToString());
            return message;
        }
    }
}
