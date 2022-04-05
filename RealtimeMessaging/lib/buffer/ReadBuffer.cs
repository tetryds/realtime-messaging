using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace tetryds.RealtimeMessaging.MemoryManagement
{
    public class ReadBuffer : IDisposable
    {
        //TODO: Dispose logic
        readonly MemoryStream memoryStream;
        readonly MemoryPool memoryPool;

        public long Length => memoryStream.Length;

        public ReadBuffer(MemoryStream memoryStream, MemoryPool memoryPool)
        {
            this.memoryStream = memoryStream;
            this.memoryPool = memoryPool;
        }

        public int Read(byte[] buffer)
        {
            return Read(buffer, 0, buffer.Length);
        }

        public int Read(byte[] buffer, int index, int count)
        {
            return memoryStream.Read(buffer, index, count);
        }

        public void Dispose()
        {
            memoryPool.Push(memoryStream);
        }

        ~ReadBuffer() => Dispose();
    }
}
