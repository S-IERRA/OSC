namespace ChatShared.Json;

public record CreateServerEvent(string Name);

public record JoinServerEvent(string InviteCode);

public record ServerEvent(Guid ServerId);

public record CreateInvite(Guid ServerId, string InviteCode);