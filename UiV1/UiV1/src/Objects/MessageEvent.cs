using ChatClient.Handlers;
using Newtonsoft.Json;

namespace ChatClient;

public enum EventType
{
    Ready,
    Message,
    Null
}

public record WebSocketMessage(OpCodes OpCode, string? Message, Events? EventType, string? Session); //uint       Sequence);