namespace ChatClient.Types;

[Flags]
public enum Permissions
{
    Member,
    Administrator,
    CanKick,
    CanBan,
    CanMute,
}

public class Server
{
    public int Id { get; set; }
    private int _ownerId;

    public required User Owner { get; set; }
    public required string Name { get; set; }

    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Banner { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Member> Members { get; set; }
    public virtual ICollection<Role> Roles { get; set; }
    public virtual ICollection<Channel> Channels { get; set; }
    public virtual ICollection<Invite> InviteCodes { get; set; }
}

public class Member
{
    public int Id { get; set; }

    private int userId;
    private int serverId;

    public Permissions Permissions { get; set; }
    public DateTime Joined { get; set; } = DateTime.UtcNow;

    public Server Server { get; set; }
    public User User { get; set; }

    public virtual ICollection<Role> Roles { get; set; }
}

public class Invite
{
    public int Id { get; set; }

    public int ServerId { get; set; }
    public string InviteCode { get; set; }
}

public class Role
{
    public int Id { get; set; }

    private int userId;
    private int serverId;

    public required Server Server { get; set; }
    public Member User { get; set; }

    public required string Name { get; set; }
    public required int Color { get; set; } //Hex

    public required Permissions Permissions { get; set; }
}