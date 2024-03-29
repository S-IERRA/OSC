﻿using System.Runtime.InteropServices;

namespace ChatShared.Types;

public record WebSocketMessage(OpCodes OpCode, string? Message, Events? EventType, Guid? Session);

[StructLayout(LayoutKind.Sequential)]
public record Packet(uint Id, uint ReplyId, int Length, string Message);