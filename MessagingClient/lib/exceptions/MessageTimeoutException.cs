using System;
using System.Runtime.Serialization;

namespace tetryds.RealtimeMessaging
{
    [Serializable]
    internal class MessageTimeoutException : Exception
    {
        public readonly int TimeoutMs;

        public MessageTimeoutException(int timeoutMs)
        {
            TimeoutMs = timeoutMs;
        }

        public override string ToString()
        {
            return $"Message timed out after {TimeoutMs} seconds.{Environment.NewLine}{base.ToString()}";
        }
    }
}