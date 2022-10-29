using System;
using Newtonsoft.Json;

namespace ChatServer.Objects;

public class HeartBeatAck
{
    [JsonProperty("last_heartbeat")] public DateTimeOffset Last { get; set; }
}