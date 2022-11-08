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