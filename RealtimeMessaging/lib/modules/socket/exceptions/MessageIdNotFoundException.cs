using System;
using System.Runtime.Serialization;

namespace tetryds.RealtimeMessaging.Network
{
    [Serializable]
    internal class MessageIdNotFoundException : Exception
    {
        public readonly int Id;

        public MessageIdNotFoundException()
        {
        }

        public MessageIdNotFoundException(int id)
        {
            Id = id;
        }

        public MessageIdNotFoundException(string message) : base(message)
        {
        }

        public MessageIdNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MessageIdNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}