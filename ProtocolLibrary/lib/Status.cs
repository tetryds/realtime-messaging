using System;
using tetryds.RealtimeMessaging.MemoryManagement;

namespace tetryds.RealtimeMessaging
{
    public enum Status : ushort
    {
        Ok = 100,
        Continue = 101,
        Cancel = 102,

        InternalError = 200,
        ConsumerError = 201,
    }
}
