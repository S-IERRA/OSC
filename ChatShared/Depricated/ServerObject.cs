using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChatShared.Types;

//Convert to combined key
[NotMapped]
public class InviteShared
{
    public Guid Id { get; set; }

    public Guid ServerId { get; set; }
    public string InviteCode { get; set; }
}
[NotMapped]
public class ServerShared
{
     public Guid Id { get; set; }
     public required Guid OwnerId { get; set; }
    
    public UserShared   Owner { get; set; }
    public required string Name  { get; set; }
    
    public string?   Description  { get; set; }
    public string?   Icon         { get; set; }
    public string?   Banner       { get; set; }
    
    public DateTimeOffset Created { get; set; } = DateTime.UtcNow;

    public virtual ICollection<MemberShared>  Members     { get; set; } = new HashSet<MemberShared>();
    public virtual ICollection<RoleShared>    Roles       { get; set; } = new HashSet<RoleShared>();
    public virtual ICollection<ChannelShared> Channels    { get; set; } = new HashSet<ChannelShared>();
    public virtual ICollection<InviteShared>  InviteCodes { get; set; } = new HashSet<InviteShared>();
}

//Remove id convert to combined key
[NotMapped]
public class MemberShared
{
    public Guid Id { get; set; }

    public Guid userId;
    public Guid serverId;
    
    public Permissions Permissions { get; set; }
    public DateTime    Joined      { get; set; } = DateTime.UtcNow;
    
    [JsonIgnore]
    public ServerShared Server { get; set; }
    public UserShared   User   { get; set; }
    
    public virtual ICollection<RoleShared> Roles { get; set; } = new HashSet<RoleShared>();
}

[Flags]
public enum Permissions
{
    Member,
    Administrator,
    CanKick,
    CanBan,
    CanMute,
}