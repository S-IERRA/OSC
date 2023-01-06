using System.Text.Json;
using System.Net.Sockets;
using System.Text.Json.Serialization;
using ChatShared.Types;
using WebSocketMessage = ChatServer.Objects.WebSocketMessage;

namespace ChatServer.Handlers
{
    public record SocketUser(Socket UnderSocket) : IDisposable 
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.Always
        };
        
        public readonly CancellationTokenSource UserCancellation = new CancellationTokenSource();

        //public IPAddress UserIp { get; set; 
        public bool IsIdentified = false;
        public string? SessionId;
        private int PacketId = 0;
        
        public delegate void WorkCompletedCallBack(string result);

        public void Dispose()
        {
            UserCancellation.Cancel();
            UnderSocket.Close();
            
            UserCancellation.Dispose();

            GC.SuppressFinalize(this);
        }

        private async Task SendData(OpCodes opCode, Events? eventType = null, string? dataSerialized = default, int replyId = 0)
        {
            if (!UnderSocket.Connected)
                Dispose();
            
            WebSocketMessage message = new(opCode, dataSerialized, eventType, default);

            string messageSerialized = JsonSerializer.Serialize(message);
            byte[] dataCompressed = GZip.Compress(messageSerialized, PacketId, replyId);

            await UnderSocket.SendAsync(dataCompressed, SocketFlags.None);
        }

        public async Task Send(OpCodes opCode) 
            => await SendData(opCode);
        
        public async Task Send(Events eventType)
            => await SendData(OpCodes.Event, eventType);
        
        public async Task Send(OpCodes opCode, Events eventType) 
            => await SendData(opCode, eventType);
        
        public async Task Send(OpCodes opCode, string message) 
            => await SendData(opCode, null, message);

        public async Task Send(OpCodes opCode, object message)
        {
            string jsonMessage = JsonSerializer.Serialize(message, Options);
            
            await SendData(opCode, null, jsonMessage);
        }
        
        public async Task Send(Events eventType, object message)
        { 
            string jsonMessage = JsonSerializer.Serialize(message);
            
            await SendData(OpCodes.Event, eventType, jsonMessage);
        }
    }
}