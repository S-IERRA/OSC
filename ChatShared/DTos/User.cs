using ChatShared.Types;

using Mapster;

namespace ChatShared.DTos;

[AdaptTo(typeof(UserShared)), GenerateMapper]
public class UserLoggedIn
{
	public required Guid Id { get; set; }
	public Guid? Session { get; set; }

	public string Username { get; set; }
	public string Email { get; set; }

	public virtual ICollection<ServerShared> Servers { get; set; } = new HashSet<ServerShared>();

	public static explicit operator UserLoggedIn(UserShared user)
		=> user.Adapt<UserLoggedIn>();
}