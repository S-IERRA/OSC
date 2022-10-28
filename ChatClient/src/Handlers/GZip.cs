using System.IO.Compression;
using System.Text;

namespace ChatClient.Handlers;

public class GZip
{
    public static byte[] Compress(string data)
    {
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            
        /*using var memStream = new MemoryStream(dataBytes);
        using var gZip = new GZipStream(memStream, CompressionMode.Compress);
        gZip.Write(dataBytes);*/

        return dataBytes;
    }

    public static string Decompress(MemoryStream memoryStream)
    {
        /* using var gZip = new GZipStream(memoryStream, CompressionMode.Decompress);
         gZip.CopyTo(memoryStream);*/
            
        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }
}