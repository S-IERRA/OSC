namespace ChatServer.Objects;

public class LoginRegisterEvent
{
    public required string Password { get; init; }

    public string? Username { get; init; }
    public string? Email { get; init; }
}