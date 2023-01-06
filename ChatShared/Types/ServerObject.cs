using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChatShared.Types;

//Convert to combined key
public class InviteShared
{
    public int Id { get; set; }

    public int ServerId { get; set; }
    public string InviteCode { get; set; }
}

public class ServerShared
{
    [NotMapped] public int Id { get; set; }
    [NotMapped] public int _ownerId;
    
    public required UserShared   Owner { get; set; }
    public required string Name  { get; set; }
    
    public string?   Description  { get; set; }
    public string?   Icon         { get; set; }
    public string?   Banner       { get; set; }
    
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public virtual ICollection<MemberShared>  Members     { get; set; }
    public virtual ICollection<RoleShared>    Roles       { get; set; }
    public virtual ICollection<ChannelShared> Channels    { get; set; }
    public virtual ICollection<InviteShared>  InviteCodes { get; set; }
}

//Remove id convert to combined key
public class MemberShared
{
    public int Id { get; set; }

    [NotMapped] public int userId;
    [NotMapped] public int serverId;
    
    public Permissions Permissions { get; set; }
    public DateTime    Joined      { get; set; } = DateTime.UtcNow;
    
    [JsonIgnore]
    public ServerShared Server { get; set; }
    public UserShared   User   { get; set; }
    
    public virtual ICollection<RoleShared> Roles { get; set; }
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