using ChatClient.Handlers;
using Newtonsoft.Json;

namespace ChatClient;

public enum EventType
{
    Ready,
    Message,
    Null
}

public record MessageEvent(
    [JsonProperty("op")]    OpCodes OpCode,
    [JsonProperty("data")]  string? Message,
    [JsonProperty("event")] Events? EventType,
    [JsonProperty("session")] string Session = "");