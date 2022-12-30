namespace ChatClient.Json;

public class SendMessageEvent
{
    public string Content { get; set; }
    public int ChannelId { get; set; }
}

public class RequestChannelMessages
{
    public int channel { get; set; }
}