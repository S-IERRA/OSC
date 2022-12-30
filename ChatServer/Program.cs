using System.IO.Compression;
using System.Net.NetworkInformation;
using System.Text;
using ChatServer.Handlers;

SocketServer2 a = new SocketServer2();
await a.Start();

//Factory factory = new Factory();
//EntityFramework2 ctx = factory.CreateDbContext();

//await ctx.Database.EnsureDeletedAsync();
//await ctx.Database.EnsureCreatedAsync();

Thread.Sleep(-1);

//AccountService ac = new AccountService();
//
/*static string Pad(byte b)
{
    return Convert.ToString(b, 2).PadLeft(8, '0');
}

byte[] compressed = GZip.Compress(1 , "test");
byte[] decompressed = GZip.Decompress(compressed);
Console.WriteLine(decompressed.Length);

int id = GZip.GetInt(decompressed);
int replyId = GZip.GetInt(decompressed, 4);
int messageLength = GZip.GetInt(decompressed, 8);

string message = Encoding.UTF8.GetString(decompressed, 12, messageLength);

Console.WriteLine($"id: {id}, replyId: {replyId}, messageLength: {messageLength}, message: {message}");

Console.WriteLine();

 //  4     4       4        ?
    //[ID][REPLYID][LENGTH](MESSAGE)

record struct Packet(int Id, int ReplyId, int Length, byte[] Message);*/

public static class GZip
{
    private static byte[] GetBytes(uint length)
    {
        byte[] bytes = new byte[4];
        bytes[0] = (byte)(length & 0xFF);
        bytes[1] = (byte)((length >> 8) & 0xFF);
        bytes[2] = (byte)((length >> 16) & 0xFF);
        bytes[3] = (byte)((length >> 24) & 0xFF);
        return bytes;
    }

    public static int GetInt(byte[] bytes, int offset = 0)
    {
        return (bytes[offset + 0] | (bytes[offset + 1] << 8) |
                (bytes[offset + 2] << 16) | (bytes[offset + 3] << 24));
    }

    public static byte[] Compress(uint id, string data, uint replyId = 0)
    {
        byte[] idBytes = GetBytes(id);
        byte[] replyIdBytes = GetBytes(replyId);
        byte[] length = GetBytes((uint)data.Length);
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);

        byte[] prepended = idBytes
            .Concat(replyIdBytes)
            .Concat(length)
            .Concat(dataBytes)
            .ToArray();
        
        using MemoryStream ms = new();
        using (GZipStream zip = new(ms, CompressionMode.Compress, true))
        {
            zip.Write(prepended, 0, prepended.Length);
        }

        return ms.ToArray();
    }

    public static byte[] Decompress(byte[] inBytes)
    {
        using MemoryStream inStream = new(inBytes);
        using GZipStream zip = new(inStream, CompressionMode.Decompress);
        using MemoryStream outStream = new();

        byte[] buffer = new byte[4096];
        int read;

        while ((read = zip.Read(buffer, 0, buffer.Length)) > 0) 
            outStream.Write(buffer, 0, read);

        return outStream.ToArray();
    }
}


/*
SocketServer2 server = new();
await server.Start();
*/
//Thread.Sleep(-1);