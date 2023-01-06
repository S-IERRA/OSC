namespace ChatShared.Types;

public record WebSocketMessage(OpCodes OpCode, string? Message, Events? EventType, string? Session); //uint       Sequence);