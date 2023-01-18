using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChatShared.Types;
[NotMapped]
public class MessageShared
{
    public Guid Id { get; set; }
    
    public required Guid AuthorId { get; set; }
    public MemberShared  Author  { get; set; }
    
    public required Guid ServerId { get; set; }
    public ServerShared   Server  { get; set; }
    
    public required Guid ChannelId { get; set; }
    [JsonIgnore] public ChannelShared  Channel { get; set; }
    
    public required string  Content { get; set; }
    
    
    public DateTimeOffset Sent { get; set; } = DateTime.UtcNow;
}