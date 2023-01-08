namespace ChatShared.Json;

public record CreateServerEvent(string Name);

public record JoinServerEvent(string InviteCode);

public record LeaveServerEvent(Guid ServerId);

public record DeleteServerEvent(Guid ServerId);

public record CreateInvite(Guid ServerId, string InviteCode);