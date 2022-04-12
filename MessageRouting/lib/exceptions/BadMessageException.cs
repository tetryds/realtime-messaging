using System;
using System.Runtime.Serialization;

namespace tetryds.RealtimeMessaging
{
    [Serializable]
    internal class BadMessageException : Exception
    {
        public Type Type { get; }

        public BadMessageException(Type type, string message) : base(message)
        {
            Type = type;
        }

        public BadMessageException(Type type, string message, Exception innerException) : base(message, innerException)
        {
            Type = type;
        }

        public override string ToString()
        {
            return $"Source module: {Type};{Environment.NewLine}{base.ToString()}";
        }
    }
}