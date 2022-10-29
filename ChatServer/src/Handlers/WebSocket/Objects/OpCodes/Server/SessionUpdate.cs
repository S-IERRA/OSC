using Newtonsoft.Json;

namespace ChatServer.Objects;

public class SessionUpdate
{
    [JsonProperty("new_session")] public required string NewSession { get; set; }
}