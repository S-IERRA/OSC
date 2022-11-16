using ChatServer.Handlers;

namespace ChatServer.Objects;

public class Identified
{
    public int UserId { get; set; }
    public DateTimeOffset CreationDate { get; set; }

    public string Email { get; set; }
    public string Username { get; set; }
    public User? Friends { get; set; }
    public ulong[]? SubscribedChannels { get; set; }
}