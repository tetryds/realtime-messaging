using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace tetryds.RealtimeMessaging.Buffer
{
    public class ReadBuffer
    {
        MemoryStream memoryStream;

        public ReadBuffer(MemoryStream memoryStream)
        {
            this.memoryStream = memoryStream;
        }

        public int Read(byte[] buffer)
        {
            return Read(buffer, 0, buffer.Length);
        }

        public int Read(byte[] buffer, int index, int count)
        {
            return memoryStream.Read(buffer, index, count);
        }
    }
}
