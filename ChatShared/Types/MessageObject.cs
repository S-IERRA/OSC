using System.Text.Json.Serialization;

namespace ChatShared.Types;

public class MessageShared
{
    public Guid Id { get; set; }
    
    public required MemberShared  Author  { get; set; }
    public required ServerShared   Server  { get; set; }
    [JsonIgnore] public ChannelShared  Channel { get; set; }
    public required string  Content { get; set; }
    
    public DateTimeOffset Sent { get; set; } = DateTime.UtcNow;
}