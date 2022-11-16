namespace ChatServer.Objects;

//For receiving data we need Session in order to find the user and verify the request, for sending it is not required.
public record WebSocketMessage(OpCodes OpCode, string? Message, Events? EventType, string? Session); //uint       Sequence);