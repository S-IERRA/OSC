namespace ChatShared.Json;

public record CreateServerEvent(string Name);

public record JoinServerEvent(string InviteCode);

public record LeaveServerEvent(int ServerId);

public record DeleteServerEvent(int ServerId);

public record CreateInvite(int ServerId, string InviteCode);