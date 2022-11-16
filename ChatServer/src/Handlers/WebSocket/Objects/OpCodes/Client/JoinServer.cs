namespace ChatServer.Objects;

public class JoinServer
{
    public int ServerId { get; set; }

    public static implicit operator int(JoinServer server) => server.ServerId;
}