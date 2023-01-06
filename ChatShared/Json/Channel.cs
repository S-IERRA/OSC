namespace ChatShared.Json;

public record SendMessageEvent(string Content, int ChannelId);

public record RequestChannelMessages(int ChannelId);