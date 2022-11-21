using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace ChatServer.Handlers
{
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
        
            using MemoryStream ms = new MemoryStream();
            using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                zip.Write(prepended, 0, prepended.Length);
            }
            return ms.ToArray(); 
        }
        
        public static byte[] Decompress(byte[] data)
        {
            using MemoryStream ms = new MemoryStream(data);
            using GZipStream zip = new GZipStream(ms, CompressionMode.Decompress);
            using MemoryStream outStream = new MemoryStream();
            
            byte[] buffer = new byte[4096];
            int read;
            
            while ((read = zip.Read(buffer, 0, buffer.Length)) > 0)
            {
                outStream.Write(buffer, 0, read);
            }
            
            return outStream.ToArray();
        }
        
        /*
        public static string Decompress(byte[] compressedBytes)
        {
            using var memStream = new MemoryStream(compressedBytes);
            using var gZip = new GZipStream(memStream, CompressionMode.Decompress);
            using var reader = new StreamReader(gZip, Encoding.UTF8);

            return reader.ReadToEnd();
        }*/
    }
}