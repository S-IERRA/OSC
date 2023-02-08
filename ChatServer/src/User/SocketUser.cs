using System.Text.Json;
using System.Net.Sockets;
using System.Text.Json.Serialization;
using ChatShared;
using ChatShared.Types;

namespace ChatServer.Handlers;

public record SocketUser(Socket UnderSocket) : IDisposable 
{
    public readonly CancellationTokenSource UserCancellation = new CancellationTokenSource();

    //public IPAddress UserIp { get; set; 
    public bool IsIdentified = false;
    public Guid? SessionId;
        
    private uint _packetId = 1;
    public uint ReplyId = 1;
        
    public void Dispose()
    {
        UserCancellation.Cancel();
        UnderSocket.Close();
            
        UserCancellation.Dispose();

        GC.SuppressFinalize(this);
    }

    private async Task SendData(OpCodes opCode, Events? eventType = null, string? dataSerialized = default, uint replyId = 0)
    {
        if (!UnderSocket.Connected)
            Dispose();
            
        WebSocketMessage socketMessage = new(opCode, dataSerialized, eventType, default);

        string messageSerialized = JsonSerializer.Serialize(socketMessage);
            
        byte[] dataCompressed = GZip.Compress(messageSerialized, _packetId++, ReplyId);

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
        string jsonMessage = JsonSerializer.Serialize(message);
            
        await SendData(opCode, null, jsonMessage);
    }
        
    public async Task Send(Events eventType, object message)
    { 
        string jsonMessage = JsonSerializer.Serialize(message);
            
        await SendData(OpCodes.Event, eventType, jsonMessage);
    }
}