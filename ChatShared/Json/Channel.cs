namespace ChatShared.Json;

public record SendMessageEvent(string Content, Guid ChannelId, Guid ServerId);

public record RequestChannelMessages(Guid ChannelId);