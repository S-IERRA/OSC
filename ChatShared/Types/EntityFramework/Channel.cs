namespace ChatShared.Types;

public class ChannelShared
{
	public required Guid Id { get; set; }

	public required Guid ServerId { get; set; }
	public ServerShared Server { get; set; }

	public required string Name { get; set; }

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

	public required Guid ChannelId { get; set; }
	public ChannelShared Channel { get; set; }

	public DateTimeOffset Created { get; set; }

	public string Content { get; set; }
}

[Flags]
public enum Permissions
{
	Member,
	CanKick,
	CanBan,
	Admin,
}