using Newtonsoft.Json;

namespace ChatServer.Objects;

public class JoinServer
{
    [JsonProperty("server_id")] public int ServerId { get; set; }

    public static implicit operator int(JoinServer server) => server.ServerId;
}