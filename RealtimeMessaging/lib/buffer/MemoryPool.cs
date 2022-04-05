using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace tetryds.RealtimeMessaging.MemoryManagement
{
    public class MemoryPool : IDisposable
    {
        //TODO: Use RecyclableMemoryStream
        //TODO: Trim big memorystream objects
        ConcurrentStack<MemoryStream> memoryStreams = new ConcurrentStack<MemoryStream>();

        public MemoryStream Pop()
        {
            if (!memoryStreams.TryPop(out MemoryStream memoryStream))
                memoryStream = new MemoryStream();
            return memoryStream;
        }

        public void Push(MemoryStream memoryStream)
        {
            if (!(memoryStream.CanRead && memoryStream.CanWrite)) return;

            memoryStream.Position = 0;
            memoryStream.SetLength(0);
            memoryStreams.Push(memoryStream);
        }

        public int Count() => memoryStreams.Count;

        public void Trim(int maxCount)
        {
            if (maxCount < 0)
                throw new ArgumentOutOfRangeException($"Trim maxCount cannot be a negative value, given value: {maxCount}");

            while (memoryStreams.Count > maxCount)
                Pop().Dispose();
        }

        public void Dispose()
        {
            while (memoryStreams.TryPop(out MemoryStream memoryStream))
                memoryStream.Dispose();
        }
    }
}
