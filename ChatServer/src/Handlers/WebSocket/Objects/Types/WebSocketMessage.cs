using ChatServer.Handlers;
using Newtonsoft.Json;

namespace ChatServer.Objects
{
    //For receiving data we need Session in order to find the user and verify the request, for sending it is not required.
    public record WebSocketMessage(
        [JsonProperty("op")]      OpCodes OpCode,
        [JsonProperty("data")]    string? Message,
        [JsonProperty("event")]   Events? EventType,
        [JsonProperty("session")] string Session = "");
        //[JsonProperty("seq")]   uint       Sequence);
}