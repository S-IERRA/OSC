using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using System.Net.Sockets;
using ChatServer.Objects;

namespace ChatServer.Handlers
{
    public record SocketUser(Socket UnderSocket) : IDisposable
    {
        public readonly CancellationTokenSource UserCancellation = new CancellationTokenSource();

        //public IPAddress UserIp { get; set; 
        public bool IsIdentified = false;
        public string SessionId;
        
        public void Dispose()
        {
            UserCancellation.Cancel();
            UnderSocket.Close();
            
            UserCancellation.Dispose();

            GC.SuppressFinalize(this);
        }

        public async Task Send(OpCodes opCode, string data = "", Events eventType = Events.Null)
        {
            string serialized = JsonConvert.SerializeObject(new WebSocketMessage(opCode, data, eventType));

            byte[] dataCompressed = GZip.Compress(serialized);

            if (UnderSocket.Connected)
            {
                await UnderSocket.SendAsync(dataCompressed, SocketFlags.None);
                return;
            }
            
            Dispose();
        }
        
        public async Task Send(Events eventName, string data = "")
        {
            string serialized = JsonConvert.SerializeObject(new WebSocketMessage(OpCodes.Event, data, eventName));

            byte[] dataCompressed = GZip.Compress(serialized);

            if (UnderSocket.Connected)
            {
                await UnderSocket.SendAsync(dataCompressed, SocketFlags.None);
                return;
            }
            
            Dispose();
        }
    }
}