using System.ComponentModel.DataAnnotations.Schema;

namespace ChatShared.Types;

[NotMapped]
public class ChannelShared
{
    public Guid Id { get; set; }

    public required string Name { get; set; }
    public required Permissions ViewPermission { get; set; }
    
    public required Guid ServerId { get; set; } // You need this
    public ServerShared Server { get; set; }

    public virtual ICollection<MessageShared> Messages { get; set; } = new HashSet<MessageShared>();
}