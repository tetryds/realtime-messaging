using System;
using System.Runtime.Serialization;

namespace tetryds.RealtimeMessaging.Network
{
    [Serializable]
    internal class SourceNotConnectedException : Exception
    {
        public readonly Guid ClientId;

        public SourceNotConnectedException()
        {
        }

        public SourceNotConnectedException(Guid clientId)
        {
            ClientId = clientId;
        }

        public SourceNotConnectedException(string message) : base(message)
        {
        }

        public SourceNotConnectedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SourceNotConnectedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}