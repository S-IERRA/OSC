using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace ChatServer.Objects;

public class ChannelObject
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [JsonProperty("messages")] public ICollection<MessageObject> Messages { get; set; }
}

public class DmChannel : ChannelObject
{
    [JsonProperty("recipient")] public UserProperties Subscriber { get; set; }
}

public class GroupChannel : ChannelObject
{
    [JsonProperty("recipients")] public List<BasicUser> Subscribers { get; set; }
}