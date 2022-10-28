using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace ChatServer.Objects;

//Todo: clean this up into different files

public class MessageObject
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public ChannelObject Channel { get; set; }
    
    [JsonProperty("author")]  public required BasicUser      Author    { get; set; }
    [JsonProperty("time")]    public required DateTimeOffset TimeStamp { get; set; }
    
    [JsonProperty("message")] public required string Message { get; set; }
}

public class ChannelObject
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [JsonProperty("messages")] public ICollection<MessageObject> Messages { get; set; }
}

public class DmChannel : ChannelObject
{
    [JsonProperty("recipient")] public Subscriber Subscribers { get; set; }
}

public class GroupChannel : ChannelObject
{
    [JsonProperty("recipients")] public List<BasicUser> Subscribers { get; set; }
}

public class ServerObject
{
    [JsonProperty("owner")] public BasicUser? Creator { get; set; }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonProperty("server_id")] public uint Id { get; init; } //Convert this to a foreign key

    [JsonProperty("name")] public required string Name { get; init; }
    [JsonProperty("icon")] public string? Icon { get; init; }
    
    public List<UserProperties> Users { get; set; }
    public List<ChannelObject>? Channels { get; set; }
}