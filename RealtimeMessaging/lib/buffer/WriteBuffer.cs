using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace tetryds.RealtimeMessaging.MemoryManagement
{
    public class WriteBuffer
    {
        MemoryStream memoryStream;

        public WriteBuffer(MemoryStream memoryStream)
        {
            this.memoryStream = memoryStream;
        }

        public void Write(byte[] data)
        {
            Write(data, 0, data.Length);
        }

        public void Write(byte[] data, int index, int count)
        {
            memoryStream.Write(data, index, count);
        }
    }
}
