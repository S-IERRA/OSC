using Newtonsoft.Json;

namespace ChatServer.Objects
{
    public struct Hello
    {
        [JsonProperty("")]
        public static uint Interval { get; set; }
    }
}