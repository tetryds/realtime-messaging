using System;
using System.Net.Sockets;
using System.Runtime.Serialization;

namespace tetryds.RealtimeMessaging.Network.Errors
{
    [Serializable]
    internal class SocketConnectionException : Exception
    {
        public readonly SocketError Error;

        public SocketConnectionException(SocketError error)
        {
            Error = error;
        }

        public SocketConnectionException(SocketError error, string message) : base(message)
        {
            Error = error;
        }

        public SocketConnectionException(SocketError error, string message, Exception innerException) : base(message, innerException)
        {
            Error = error;
        }

        protected SocketConnectionException(SocketError error, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Error = error;
        }
    }
}