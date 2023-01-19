namespace ChatShared.DTos;


//Todo: Fix naming
public class UserShared
{
    public required Guid Id { get; set; }
    public Guid? Session { get; set; }
    
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }

    public virtual ICollection<ServerShared> Servers { get; set; } = new HashSet<ServerShared>();
}

public class ServerMemberShared
{
    public required Guid UserId { get; set; }
    public UserShared User { get; set; }
    
    public required Guid ServerId { get; set; }
    public ServerShared Server { get; set; }

    public Permissions Permissions { get; set; }

    public virtual ICollection<RoleShared> Roles { get; set; } = new HashSet<RoleShared>();
}


public class ServerShared
{
    public required Guid Id { get; set; }
    public Guid Session { get; set; }
    
    public required Guid OwnerId { get; set; }
    public UserShared Owner { get; set; } 

    public required string Name { get; set; }

    public virtual ICollection<RoleShared>            Roles { get; set; } = new HashSet<RoleShared>();
    public virtual ICollection<ServerMemberShared>  Members { get; set; } = new HashSet<ServerMemberShared>();
    public virtual ICollection<ChannelShared>     Channels { get; set; } = new HashSet<ChannelShared>(); 
    public virtual ICollection<InviteShared>    InviteCodes { get; set; } = new HashSet<InviteShared>();
}

public class ChannelShared
{
    public required Guid Id { get; set; }

    public required Guid ServerId { get; set; }
    public ServerShared Server { get; set; }

    public string Name { get; set; }
    
    public Permissions ViewPermissions { get; set; }

    public virtual ICollection<MessageShared> Messages { get; set; } = new HashSet<MessageShared>();
}

public class MessageShared
{
    public required Guid Id { get; set; }
    
    public required Guid AuthorId { get; set; }
    public ServerMemberShared Author { get; set; }

    public required Guid ServerId { get; set; }
    public ServerShared Server { get; set; }
    
    public required Guid  ChannelId { get; set; }
    public ChannelShared Channel { get; set; }
    
    public DateTimeOffset Created { get; set; }
    
    public string Content { get; set; }
}

public class RoleShared
{
    public required Guid Id { get; set; }
    
    public required uint HexColour { get; set; }
    
    public required Guid ServerId { get; set; }
    public ServerShared Server { get; set; }
}

public class InviteShared
{
    public Guid Id { get; set; }

    public Guid ServerId { get; set; }
    public string InviteCode { get; set; }
}

[Flags]
public enum Permissions
{
    Member,
    CanKick,
    CanBan,
    Admin,
}
