using Newtonsoft.Json;

namespace ChatServer.Objects
{
    public class HeartBeat
    {
        [JsonProperty("session_id")]
        public string SessionId { get; init; }
    }
}