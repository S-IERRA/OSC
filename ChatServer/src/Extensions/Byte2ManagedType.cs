using System.Runtime.InteropServices;
using ChatShared.Types;

namespace ChatServer.Extensions;

public static class Byte2ManagedType
{
    public static T? ToStruct<T>(this byte[] bytes)
    {
        unsafe
        {
            fixed (byte* pBytes = bytes)
            {
                return (T?)Marshal.PtrToStructure(new nint(pBytes), typeof(T));
            }
        }
    }
}