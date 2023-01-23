namespace ChatShared.Types;

public class ServerShared
{
	public required Guid Id { get; set; }
	public Guid Session { get; set; }

	public required Guid OwnerId { get; set; }
	public UserShared Owner { get; set; }

	public required string Name { get; set; }

	public virtual ICollection<RoleShared> Roles { get; set; } = new HashSet<RoleShared>();
	public virtual ICollection<ServerMemberShared> Members { get; set; } = new HashSet<ServerMemberShared>();
	public virtual ICollection<ChannelShared> Channels { get; set; } = new HashSet<ChannelShared>();
	public virtual ICollection<InviteShared> InviteCodes { get; set; } = new HashSet<InviteShared>();
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

public class InviteShared
{
	public required Guid Id { get; set; }

	public required Guid ServerId { get; set; }
	public required string InviteCode { get; set; }
}


public class RoleShared
{
    public required Guid Id { get; set; }

    public required uint HexColour { get; set; }

    public required Guid ServerId { get; set; }
    public ServerShared Server { get; set; }
}