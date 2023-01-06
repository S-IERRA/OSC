using System.Text.Json.Serialization;

namespace ChatShared.Types;

public class MessageShared
{
    public int Id { get; set; }
    
    public required MemberShared  Author  { get; set; }
    public required ServerShared  Server  { get; set; }
    [JsonIgnore] public ChannelShared Channel { get; set; }
    public required string  Content { get; set; }
    
    public DateTime Sent { get; set; } = DateTime.UtcNow;
}