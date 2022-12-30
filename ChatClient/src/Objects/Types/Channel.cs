namespace ChatClient.Types;

public class Channel
{
    public int Id { get; set; }

    public required string Name { get; set; }
    public required Permissions ViewPermission { get; set; }

    public required Server Server { get; set; }

    public virtual List<Message> Messages { get; set; }
}

public class Message
{
    public int Id { get; set; }

    public required Member Author { get; set; }
    public required Server Server { get; set; }
    public Channel? Channel { get; set; }
    public required string Content { get; set; }

    public DateTime Sent { get; set; }
}