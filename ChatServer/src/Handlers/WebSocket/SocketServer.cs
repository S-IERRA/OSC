﻿using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ChatServer.Extensions;
using ChatServer.Objects;

using ChatShared;
using ChatShared.Json;
using ChatShared.Types;
using Microsoft.EntityFrameworkCore;

using Serilog;

namespace ChatServer.Handlers;

public class SocketServer : IDisposable
{
    private static readonly ArrayPool<int> ArrayPool = ArrayPool<int>.Create();

    private static readonly Factory Factory = new Factory();
    private static readonly CancellationTokenSource Cts = new();

	private static readonly Socket     Listener = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private static readonly IPEndPoint EndPoint = new(IPAddress.Loopback, 8787);
    
    private SocketState _state = SocketState.Undefined;
        
    private bool CanRun() => !Cts.Token.IsCancellationRequested && _state is SocketState.Connected;
        
    private static DateTime GetCurrentTime => DateTime.Now;
    
    public async Task Start()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.Seq("http://localhost:5341/")
            .CreateLogger();
        
        Listener.Bind(EndPoint);
        Listener.Listen(32);
        
        Log.Information("Server started");

        _state = SocketState.Connected;
        
        while (CanRun())
        {
            Socket socket = await Listener.AcceptAsync();
			IPEndPoint?  ip = (IPEndPoint)socket.RemoteEndPoint; 

            SocketUser socketUser = new(socket);
            
            Log.Information($"New connection from {ip}");
            
            _ = Task.Run(() => VirtualUserHandler(socketUser), socketUser.UserCancellation.Token);
        }
    }

    //Todo: optimisation and clean-up phase
    private async Task VirtualUserHandler(SocketUser socketUser)
    {
        byte[] localBuffer = new byte[512];

        bool isTimedOut = false,
            receivedAck = false;
        
        uint packetId = 1;

        async Task HeartBeat()
        {
            for (;;)
            {
                await socketUser.Send(OpCodes.HeartBeat);
                if (socketUser.UserCancellation.IsCancellationRequested)
                    return;
                
                DateTimeOffset nextAck = DateTimeOffset.Now + TimeSpan.FromSeconds(10);

                while (GetCurrentTime < nextAck)
                {
                    if (receivedAck)
                        break;

                    await 2;
                }

                if (!receivedAck)
                {
                    //if(socketUser.IsIdentified)
                      //  await userDbService.LogOut();

                    await socketUser.Send(OpCodes.ConnectionClosed);
                    socketUser.Dispose();

                    return;
                }
                    
                receivedAck = false;
                    
                await 5;
            }
        }

        //_ = Task.Run(HeartBeat, Cts.Token);
        
        //Receive Data
        
        while (CanRun())
        {
            if (isTimedOut)
                return; //Convert this to an actual timeout
            
            //Todo: memory leak, use shared arrays
            MemoryStream dataStream = new();

            do
            {
                int totalReceived = await socketUser.UnderSocket.ReceiveAsync(localBuffer, SocketFlags.None);
                dataStream.Write(localBuffer, 0, totalReceived);
            }
            while (socketUser.UnderSocket.Available > 0);

            byte[] decompressedBytes = await GZip.Decompress(dataStream.ToArray());
            await dataStream.DisposeAsync();

            for (int totalRead = 0; decompressedBytes.Length - totalRead > 0; packetId++)
            {
                //move this to the deserializer
                socketUser.ReplyId = GZip.Byte2UInt(decompressedBytes, totalRead);
                int length = GZip.Byte2Int(decompressedBytes, totalRead + 8);
                
                string rawMessage = Encoding.UTF8.GetString(decompressedBytes, totalRead + 12, length);
                totalRead += length + 12;
                
                if (!JsonHelper.TryDeserialize<WebSocketMessage>(rawMessage, out var socketMessage))
                {
                    await socketUser.Send(OpCodes.InvalidRequest, ErrorMessages.MalformedJson); 
                    
                    Log.Warning($"Malformed Json: {rawMessage}");
                    continue;
                }
                
                //This opcode handling is a mess
                if (socketMessage.OpCode == OpCodes.HeartBeatAck)
                {
                    receivedAck = true;
                    continue;
                }
                
                EntityFramework context = Factory.CreateDbContext();
                AccountService userDbService = new(context, socketUser);

                if (!socketUser.IsIdentified)
                {
                    if (!JsonHelper.TryDeserialize<LoginRegisterEvent>(socketMessage.Message, out var lrEvent))
                    {
                        await socketUser.Send(OpCodes.InvalidRequest, ErrorMessages.MalformedJson);
                        
                        Log.Warning($"Malformed Login Register Json: {socketMessage.Message}");
                        continue;
                    }
                    
                    switch (socketMessage.OpCode)
                    {
                        case OpCodes.Identify:
                            await userDbService.Login(lrEvent);
                            continue;

                        case OpCodes.Register:
                            await userDbService.Register(lrEvent);
                            continue;
                    }
                }

                //Todo: Potentially huge vulnerability due to ip spoofing
                Guid? sessionToken = socketUser.SessionId ?? socketMessage.Session;
                
                //Get user by session
                //This seems quite inefficient as we are querying the database fore each request
                if (await context.Users.FirstOrDefaultAsync(x => x.Session == sessionToken) is not { } user)
                {
                    await socketUser.Send(OpCodes.InvalidRequest, "Invalid session");
                    continue;
                }
                
                //Todo: Implement MediatR
                switch (socketMessage.OpCode)
                {
                    case OpCodes.CreateServer:
                        await userDbService.CreateServer(socketMessage.Message, user);
                        continue;

                    case OpCodes.DeleteServer:
                        await userDbService.DeleteServer(socketMessage.Message, user);
                        continue;

                    case OpCodes.JoinServer:
                        await userDbService.JoinServer(socketMessage.Message, user);
                        continue;

                    case OpCodes.LeaveServer:
                        await userDbService.LeaveServer(socketMessage.Message, user);
                        continue;
                    
                    case OpCodes.SendMessage:
                        await userDbService.SendMessage(socketMessage.Message, user);
                        continue;
                    
                    case OpCodes.RequestChannelMessages:
                        await userDbService.GetChannelMessages(socketMessage.Message);
                        continue;
                    
                    case OpCodes.CreateServerInvite:
                        await userDbService.CreateInvite(socketMessage.Message, user);
                        continue;

                    case OpCodes.SubscribeServerMessages:
						await userDbService.SubscribeServer(socketMessage.Message, (IPEndPoint)socketUser.UnderSocket.RemoteEndPoint);
						continue;

                        //TODO: oh myg od
					case OpCodes.UnsubscribeServerMessages:
						continue;
				}
                
                
                context.Dispose();
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) 
            return;
        
        Listener.Dispose();
        Cts.Dispose();

        Log.Information("Server stopped");
        Log.CloseAndFlush();
    }

    ~SocketServer()
    {
        Dispose(false);
    }
}