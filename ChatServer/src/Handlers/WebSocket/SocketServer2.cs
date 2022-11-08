﻿using System.Net;
using System.Net.Sockets;
using System.Text;
using ChatServer.Extensions;
using ChatServer.Objects;
using Newtonsoft.Json;

namespace ChatServer.Handlers;

//Todo: in V2 remove some of the null checks replace with Empty statements
public class SocketServer2 : IDisposable
{
    private static readonly JsonSerializerSettings SerializerSettings = new() { Error = (se, ev)
            => { ev.ErrorContext.Handled = true; }
    };

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

                DateTimeOffset nextAck = DateTimeOffset.Now + TimeSpan.FromSeconds(5);

                while (GetCurrentTime < nextAck)
                {
                    if (receivedAck)
                        break;

                    await 2;
                }

                if (!receivedAck)
                {
                    if(socketUser.IsIdentified)
                        Console.WriteLine();//await context.LogOut(client);

                    await socketUser.Send(OpCodes.ConnectionClosed);
                    socketUser.Dispose();
                        
                    return;
                }
                    
                receivedAck = false;
                    
                await 5;
            }
        }

        _ = Task.Run(HeartBeat, Cts.Token);
        
        //Receive Data

        while (CanRun())
        {
            if (isTimedOut)
                return; //Convert this to an actual timeout

            using MemoryStream dataStream = new();

            do
            {
                int received = await socketUser.UnderSocket.ReceiveAsync(localBuffer, SocketFlags.None);

                dataStream.Write(localBuffer, 0, received);
            } while (socketUser.UnderSocket.Available > 0);

            string rawMessage = Encoding.UTF8.GetString(dataStream.ToArray()); //Utilise Gzip decompression
            if (JsonConvert.DeserializeObject<WebSocketMessage>(rawMessage) is not { } socketMessage)
            {
                await socketUser.Send(OpCodes.InvalidRequest);
                continue;
            }
            
            Console.WriteLine($"[SERVER] Received: {socketMessage.OpCode} {socketMessage.Message}");

            if (!socketUser.IsIdentified)
            {
                if (JsonConvert.DeserializeObject<LoginRegisterEvent>(socketMessage.Message, SerializerSettings) is not { } lrEvent)
                {
                    await socketUser.Send(OpCodes.InvalidRequest);
                    break;
                }
                
                switch (socketMessage.OpCode)
                {
                    case OpCodes.Identify:
                    {
                        
                        break;
                    }
                    
                    case OpCodes.Register:
                    {
                        break;
                    }
                }
            }
            
            //Get user by session
            
            //Check permissions
            switch (socketMessage.OpCode)
            {
                case OpCodes.HeartBeat:
                    receivedAck = true;
                    break;
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