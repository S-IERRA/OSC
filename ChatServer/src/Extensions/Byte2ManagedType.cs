using System.Runtime.InteropServices;

namespace ChatServer.Extensions;

public static class Byte2ManagedType
{
    public static T? ToStruct<T>(this byte[] bytes)
    {
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        T? theStructure = (T?)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        handle.Free();
        return theStructure;
    }
}