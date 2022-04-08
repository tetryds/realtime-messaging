using System;
using System.Runtime.Serialization;

namespace tetryds.RealtimeMessaging.Network.Exceptions
{
    [Serializable]
    public class RemoteNotConnectedException : Exception
    {
        public readonly Guid ClientId;

        public RemoteNotConnectedException()
        {
        }

        public RemoteNotConnectedException(Guid clientId)
        {
            ClientId = clientId;
        }

        public RemoteNotConnectedException(string message) : base(message)
        {
        }

        public RemoteNotConnectedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RemoteNotConnectedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override string ToString()
        {
            return $"Remote Id: {ClientId};{Environment.NewLine}{base.ToString()}";
        }
    }
}