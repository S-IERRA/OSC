using System.Text.Json;
using System.Net.Sockets;
using System.Text.Json.Serialization;
using ChatServer.Objects;

namespace ChatServer.Handlers
{
    public record SocketUser(Socket UnderSocket) : IDisposable 
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        
        public readonly CancellationTokenSource UserCancellation = new CancellationTokenSource();

        //public IPAddress UserIp { get; set; 
        public bool IsIdentified = false;
        public string? SessionId;
        
        public void Dispose()
        {
            UserCancellation.Cancel();
            UnderSocket.Close();
            
            UserCancellation.Dispose();

            GC.SuppressFinalize(this);
        }

        private async Task SendData(OpCodes opCode, Events? eventType = null, string? dataSerialized = default)
        {
            if (!UnderSocket.Connected)
                Dispose();
            
            WebSocketMessage message = new WebSocketMessage(opCode, dataSerialized, eventType, default);

            string messageSerialized = JsonSerializer.Serialize(message, Options);
            byte[] dataCompressed = GZip.Compress(messageSerialized);

            await UnderSocket.SendAsync(dataCompressed, SocketFlags.None);
        }

        public async Task Send(OpCodes opCode) 
            => await SendData(opCode);
        
        public async Task Send(Events eventType)
            => await SendData(OpCodes.Event, eventType);
        
        public async Task Send(OpCodes opCode, Events eventType) 
            => await SendData(opCode, eventType);
        
        public async Task Send(OpCodes opCode, string? message) 
            => await SendData(opCode, null, message);

        public async Task Send(OpCodes opCode, object message)
        {
            string jsonMessage = JsonSerializer.Serialize(message);
            
            await SendData(opCode, null, jsonMessage);
        }
        
        public async Task Send(Events eventType, object message)
        { 
            string jsonMessage = JsonSerializer.Serialize(message);
            
            await SendData(OpCodes.Event, eventType, jsonMessage);
        }
    }
}