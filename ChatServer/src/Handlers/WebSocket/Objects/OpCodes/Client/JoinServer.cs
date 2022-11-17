namespace ChatServer.Objects;

public class JoinServerEvent
{
    public string InviteCode { get; set; }
}

//Move this to a different file
public class LeaveServerEvent
{
    public int ServerId { get; set; }
}

public class DeleteServerEvent
{
    public int ServerId { get; set; }
}