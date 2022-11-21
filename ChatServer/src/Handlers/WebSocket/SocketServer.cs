using System.Net;
using System.Net.Sockets;
using System.Text;
using ChatServer.Extensions;
using ChatServer.Objects;
using Microsoft.EntityFrameworkCore;

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
        Listener.Bind(EndPoint);
        Listener.Listen(32);
        
        _state = SocketState.Connected;

        while (CanRun())
        {
            Socket newClient = await Listener.AcceptAsync(Cts.Token);
            SocketUser socketUser = new SocketUser(newClient);

            /*todo: CRITICAL: Host can have more than 1 session resulting in a crash due to hashmap only supporting 1 of the same input
             This is specifically a large issue when a user wants a local hosted bot or alt account to be able to connect to the server
            if (newClient.RemoteEndPoint is not null) 
                ConnectedIps.Add(newClient.RemoteEndPoint);
                */
            
            Console.WriteLine("[SERVER] Accepted new connection");
            
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
                    
                Console.WriteLine("[HEARTBEAT] Sent Heartbeat");

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

                    Console.WriteLine("Connection closed");
                    return;
                }
                    
                receivedAck = false;
                    
                await 5;
            }
        }
        
        //RateLimit
        async Task RateLimit()
        {
            await 1;
            for (;;)
            {
                return;
            }
        }

        //_ = Task.Run(HeartBeat, Cts.Token);
        
        //Receive Data

        while (CanRun())
        {
            if (isTimedOut)
                return; //Convert this to an actual timeout

            using MemoryStream dataStream = new();

            /*
            do
            {
                int received = socketUser.UnderSocket.Receive(localBuffer, SocketFlags.None);
                Console.WriteLine(received);

                dataStream.Write(localBuffer, 0, received);
            } 
            while (socketUser.UnderSocket.Available > 0);*/
            
            //Block reading
            int totalReceived = socketUser.UnderSocket.Receive(localBuffer, SocketFlags.None);
            dataStream.Write(localBuffer, 0, totalReceived);

            byte[] decompressedBytes = GZip.Decompress(dataStream.ToArray());
            int length = GZip.GetLength(decompressedBytes);
            int length2 = GZip.GetLength(decompressedBytes, length + 4);

            string data = Encoding.UTF8.GetString(decompressedBytes, 4, length);
            string data2 = Encoding.UTF8.GetString(decompressedBytes, length + 8, length2);
            
            Console.WriteLine(data);
            Console.WriteLine(data2);

            continue;
            string rawMessage = "";

            if (!JsonHelper.TryDeserialize<WebSocketMessage>(rawMessage, out var socketMessage))
            {
                await socketUser.Send(OpCodes.InvalidRequest, ErrorMessages.MalformedJson);
                Console.WriteLine("Malformed Json");
                continue;
            }

            //This opcode handling is a bit fucking wack
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
            
            //Get user by session
            //This seems quite inefficient as we are querying the database fore each request
            if (await context.Users.FirstOrDefaultAsync(x => x.Session == socketMessage.Session) is not { } user)
            {
                await socketUser.Send(OpCodes.InvalidRequest, "Invalid session");
                continue;
            }
            
            //Todo: Check permissions
            //Todo: clean this up
            //Todo: possibly move messga and user to AccountService?
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
        
        //Dispose objects
    }

    ~SocketServer2()
    {
        Dispose(false);
    }
}