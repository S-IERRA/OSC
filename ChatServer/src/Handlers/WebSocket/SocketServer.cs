using System.Net;
using System.Net.Sockets;
using System.Text;
using ChatServer.Extensions;
using ChatServer.Objects;
using ChatShared.Types;
using Microsoft.EntityFrameworkCore;

using Serilog;
using WebSocketMessage = ChatServer.Objects.WebSocketMessage;

namespace ChatServer.Handlers;

//Todo: in V2 remove some of the null checks replace with Empty statements
//Todo: clean this up in v2, get pre-alpha working
public class SocketServer2 : IDisposable
{
    private static readonly Factory Factory = new Factory();
    private static readonly CancellationTokenSource Cts = new();

    private static readonly HashSet<EndPoint> ConnectedIps = new();
    
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
            EndPoint?  ip = socket.RemoteEndPoint;

            SocketUser socketUser = new(socket);
            if (ip == null || ConnectedIps.Contains(ip))
            {
                socket.Close();
                continue;
            }
            
            ConnectedIps.Add(ip);
            
            Log.Information($"New connection from {ip}");
            
            _ = Task.Run(() => VirtualUserHandler(socketUser), socketUser.UserCancellation.Token);
        }
    }

    private async Task VirtualUserHandler(SocketUser socketUser)
    {
        EntityFramework2 context = Factory.CreateDbContext();
        AccountService userDbService = new(context, socketUser);
        
        byte[] localBuffer = new byte[512];

        bool isTimedOut = false,
            receivedAck = false;

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
                    if(socketUser.IsIdentified)
                        await userDbService.LogOut();

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
        MemoryStream dataStream = new();
        
        while (CanRun())
        {
            if (isTimedOut)
                return; //Convert this to an actual timeout

            do
            {
                int totalReceived = await socketUser.UnderSocket.ReceiveAsync(localBuffer, SocketFlags.None);
                dataStream.Write(localBuffer, 0, totalReceived);
            }
            while (socketUser.UnderSocket.Available > 0);

            byte[] decompressedBytes = await GZip.Decompress(dataStream.ToArray());
            Console.WriteLine(decompressedBytes.Length);

            for (int totalRead = 0; decompressedBytes.Length - totalRead > 0;)
            {
                //Ignore this for now, replies will come in a future version
                int id = GZip.Byte2Int(decompressedBytes, totalRead);
                int replyId = GZip.Byte2Int(decompressedBytes, totalRead + 4);
                int length = GZip.Byte2Int(decompressedBytes, totalRead + 8);
                
                string rawMessage = Encoding.UTF8.GetString(decompressedBytes, totalRead + 12, length);
                totalRead += length + 4;
                
                if (!JsonHelper.TryDeserialize<WebSocketMessage>(rawMessage, out var socketMessage))
                {
                    await socketUser.Send(OpCodes.InvalidRequest, ErrorMessages.MalformedJson); 
                    
                    Log.Warning($"Malformed Json: {rawMessage}");
                    continue;
                }
                
                //This opcode handling is a bit fucking wack
                //Rewrite this fucking mess
                if (socketMessage.OpCode == OpCodes.HeartBeatAck)
                {
                    receivedAck = true;
                    continue;
                }

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

                //Potentially huge vulnerability due to ip spoofing
                string? sessionToken = socketUser.SessionId ?? socketMessage.Session;
                
                //Get user by session
                //This seems quite inefficient as we are querying the database fore each request
                if (await context.Users.FirstOrDefaultAsync(x => x.Session == sessionToken) is not { } user)
                {
                    await socketUser.Send(OpCodes.InvalidRequest, "Invalid session");
                    continue;
                }

                //Todo: Check permissions
                //Todo: clean this up
                //Todo: possibly move message and user to AccountService?
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
                }
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
        
        ConnectedIps.Clear();
    }

    ~SocketServer2()
    {
        Dispose(false);
    }
}