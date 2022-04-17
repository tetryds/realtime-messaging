using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace tetryds.RealtimeMessaging
{
    public class MessageHandler
    {
        public readonly Message Original;
        public readonly List<Message> Received;

        Action<Message> MessageReceivedCallback;

        AutoResetEvent endEvent = new AutoResetEvent(false);

        public MessageHandler(Message original)
        {
            Original = original;
        }

        public MessageHandler(Message original, Action<Message> messageReceived) : this(original)
        {
            MessageReceivedCallback = r => InvokeReceivedCallback(r, messageReceived);
        }

        public void AddResponse(Message response)
        {
            lock (Received)
                Received.Add(response);

            MessageReceivedCallback?.Invoke(response);
        }

        public void CloseHandler()
        {
            endEvent.Set();
        }

        public MessageHandler Wait(int timeoutMs)
        {
            return Wait(timeoutMs, false);
        }

        public MessageHandler Wait(int timeoutMs, bool ignore_failure)
        {
            bool skipped = endEvent.WaitOne(timeoutMs);
            if (!ignore_failure && skipped)
                throw new MessageTimeoutException(timeoutMs);
            return this;
        }

        private void InvokeReceivedCallback(Message received, Action<Message> callback)
        {
            try
            {
                callback.Invoke(received);
            }
            catch
            {
                // TODO: Log error or call error callback
            }
        }
    }
}
