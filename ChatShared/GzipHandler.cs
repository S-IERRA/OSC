﻿using System.IO.Compression;
using System.Text;

namespace ChatShared;

public static class GZip
{
    //[ID][REPLYID=0][LENGTH][DATA]
    private static byte[] Int2Byte(int number)
    {
        byte[] bytes = new byte[4];
        bytes[0] = (byte)(number & 0xFF);
        bytes[1] = (byte)((number >> 8) & 0xFF);
        bytes[2] = (byte)((number >> 16) & 0xFF);
        bytes[3] = (byte)((number >> 24) & 0xFF);
        return bytes;
    }

    private static byte[] UInt2Byte(uint number)
    {
        byte[] bytes = new byte[4];
        bytes[0] = (byte)(number & 0xFF);
        bytes[1] = (byte)((number >> 8) & 0xFF);
        bytes[2] = (byte)((number >> 16) & 0xFF);
        bytes[3] = (byte)((number >> 24) & 0xFF);
        return bytes;
    }

    public static int Byte2Int(byte[] bytes, int offset = 0)
    {
        return (bytes[offset + 0] | (bytes[offset + 1] << 8) | (bytes[offset + 2] << 16) |
                (bytes[offset + 3] << 24));
    }

    public static uint Byte2UInt(byte[] bytes, int offset = 0)
    {
        return (uint)(bytes[offset + 0] | (bytes[offset + 1] << 8) | (bytes[offset + 2] << 16) |
                      (bytes[offset + 3] << 24));
    }

    public static byte[] Compress(string data, uint id, uint replyId = 0)
    {
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        byte[] length = Int2Byte(data.Length);
        byte[] idBytes = UInt2Byte(id);
        byte[] replyIdBytes = UInt2Byte(replyId);

        byte[] prepended = idBytes.Concat(replyIdBytes).Concat(length).Concat(dataBytes).ToArray();

        using MemoryStream ms = new();
        using (GZipStream zip = new(ms, CompressionMode.Compress, true))
        {
            zip.Write(prepended, 0, prepended.Length);
        }

        return ms.ToArray();
    }

    public static async Task<byte[]> Decompress(byte[] inBytes)
    {
        using MemoryStream inStream = new(inBytes);
        await using GZipStream zip = new(inStream, CompressionMode.Decompress);
        using MemoryStream outStream = new();

        Memory<byte> buffer = new(new byte[4096]);
        int read;
        
        while ((read = await zip.ReadAsync(buffer)) > 0)
            await outStream.WriteAsync(buffer[..read]);
        
        return outStream.ToArray();
    }
    
    /*
    public static byte[] Decompress2(ref byte[] inBytes)
    {
        using MemoryStream inStream = new(inBytes);
        using GZipStream zip = new(inStream, CompressionMode.Decompress);
        using MemoryStream outStream = new();

        Memory<byte> buffer = new(new byte[4096]);
        int read;
        
        while ((read = zip.Read(buffer)) > 0)
            outStream.Write(buffer[..read]);
        
        return outStream.ToArray();
    }*/
}