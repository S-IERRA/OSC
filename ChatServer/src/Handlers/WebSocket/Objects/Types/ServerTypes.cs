using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace ChatServer.Objects;

public class MessageObject
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public ChannelObject Channel { get; set; }
    
    [JsonProperty("author")]  public required BasicUser      Author    { get; set; }
    [JsonProperty("time")]    public required DateTimeOffset TimeStamp { get; set; }
    
    [JsonProperty("message")] public required string Message { get; set; }
}

public class ServerObject
{
    [JsonProperty("owner")] public required BasicUser Creator { get; set; }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonProperty("server_id")] public int Id { get; init; }

    [JsonProperty("name")] public required string Name { get; init; }
    [JsonProperty("icon")] public string? Icon { get; init; }
    
    [JsonIgnore] public List<UserProperties>   Users { get; set; }
    [JsonIgnore] public List<ChannelObject> Channels { get; set; }
    [JsonIgnore] public List<UserProperties>  BannedUsers { get; set; }

    public static implicit operator int(ServerObject serverObject) => serverObject.Id;
}