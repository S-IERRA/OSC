namespace ChatShared.Types;

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