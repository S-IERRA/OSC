using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using K4os.Compression.LZ4.Internal;

namespace ChatServer.Handlers
{
    //  4     4       4        ?
    //[ID][REPLYID][LENGTH](MESSAGE)
    /*
     * message -> reply
     */
    
    public static class GZip
    {
        private static byte[] GetLength(int length)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)(length & 0xFF);
            bytes[1] = (byte)((length >> 8) & 0xFF);
            bytes[2] = (byte)((length >> 16) & 0xFF);
            bytes[3] = (byte)((length >> 24) & 0xFF);
            return bytes;
        }

        public static int GetLength(byte[] bytes, int offset = 0)
        {
            return (bytes[offset + 0] | (bytes[offset + 1] << 8) | (bytes[offset + 2] << 16) | (bytes[offset + 3] << 24));
        }
        
        public static byte[] Compress(string data)
        {
            byte[] length = GetLength(data.Length);
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        
            byte[] prepended = length.Concat(dataBytes).ToArray();
        
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
    }
} 