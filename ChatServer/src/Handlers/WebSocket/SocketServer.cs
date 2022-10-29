using System.Net;
using System.Net.Sockets;
using ChatServer.Extensions;
using ChatServer.Objects;
using Newtonsoft.Json;

namespace ChatServer.Handlers
{
    public class SocketServer : IDisposable
    {
        //This might cause lots of issues lmao
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings { Error = (se, ev) => { ev.ErrorContext.Handled = true; } };

        public static readonly EntityFrameworkOrm DataBase = new EntityFrameworkOrm();
        
        private static readonly Socket Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly IPEndPoint EndPoint = new IPEndPoint(IPAddress.Loopback, 8787);
        
        private static readonly CancellationTokenSource Cts = new CancellationTokenSource();
        
        private SocketState _state = SocketState.Undefined;
        
        private bool IsConnected() => _state is SocketState.Connected;
        private bool IsCancelled() => Cts.Token.IsCancellationRequested;
        
        private DateTime GetCurrentTime() => DateTime.Now;

        public async Task Start()
        {
            Listener.Bind(EndPoint);

            Listener.Listen(32);
            _state = SocketState.Connected;

            while (IsConnected() && !IsCancelled())
            {
                Socket newClient = await Listener.AcceptAsync(Cts.Token);
                SocketUser socketUser = new SocketUser(newClient);

                Console.WriteLine("[SERVER] Accepted new connection");
                
                _ = Task.Run(() => VirtualUserHandler(socketUser), socketUser.UserCancellation.Token);
            }
        }
        
        public void Dispose()
        {
            Listener.Close();
            
            Cts.Cancel();
            Cts.Dispose();
            
            GC.SuppressFinalize(this);
        }

        private async Task VirtualUserHandler(SocketUser client)
        {
            bool isTimedOut = false,
                receivedAck = false;
 
            int timeout = 10,
                requests = 0,
                previousTimeouts = 0;

            // thread.sleep(-1) if spammed enough - CRITICAL / preferably rewrite this method
            async Task RateLimit()
            {
                for (;;)
                {
                    DateTimeOffset blockScan = DateTimeOffset.Now + TimeSpan.FromSeconds(15);
                    while (GetCurrentTime() < blockScan)
                    {
                        await 5;

                        if (requests < (requests / 15) + 1)
                        {
                            previousTimeouts--;
                            isTimedOut = false;

                            continue;
                        }

                        isTimedOut = true;
                        //requests

                        return;
                    }
                }
            }
            
            //Change the client session every n amount of heart beats
            async Task HeartBeat()
            {
                for (;;)
                {
                    await client.Send(OpCodes.HeartBeat);
                    if (client.UserCancellation.IsCancellationRequested)
                        return;
                    
                    Console.WriteLine("[HEARTBEAT] Sent Heartbeat");

                    DateTimeOffset nextAck = DateTimeOffset.Now + TimeSpan.FromSeconds(5);

                    while (GetCurrentTime() < nextAck)
                    {
                        if (receivedAck)
                            break;

                        await 2;
                    }

                    if (!receivedAck)
                    {
                        if(client.IsIdentified)
                            await DataBase.LogOut(client);

                        await client.Send(OpCodes.ConnectionClosed);
                        client.Dispose();
                        
                        return;
                    }
                    
                    receivedAck = false;
                    
                    await 5;
                }
            }

            _ = Task.Run(HeartBeat, Cts.Token);
           //Task.Run(RateLimit);

           byte[] localBuffer = new byte[512];
           
           while (IsConnected() && !IsCancelled())
           {
               if (isTimedOut)
                   await timeout;
               
               using MemoryStream stream = new MemoryStream();

                do
                {
                    int received = await client.UnderSocket.ReceiveAsync(localBuffer, SocketFlags.None);
                    stream.Write(localBuffer, 0, received);
                } 
                while (client.UnderSocket.Available > 0);

                string decompressedData = GZip.Decompress(stream);
                
                if (JsonConvert.DeserializeObject<WebSocketMessage>(decompressedData) is not {} message) 
                {
                    await client.Send(OpCodes.InvalidRequest);
                    continue;
                }

                requests++;

                if (message.OpCode == OpCodes.HeartBeatAck)
                {
                    receivedAck = true;
                    Console.WriteLine("[HEARTBEAT] Received ACK");
                    continue;
                }

                VirtualPacketHandler(message, client);
            }
        }
        
        private async Task VirtualPacketHandler(WebSocketMessage webSocketMessage, SocketUser user)
        {
            Console.WriteLine($"[OPCODE] {webSocketMessage.OpCode}");
            switch (webSocketMessage.OpCode)
            {
                //LRS
                //Can be exploited by mass requests if logged out
                case OpCodes.Register when !user.IsIdentified:
                {
                    if (JsonConvert.DeserializeObject<LoginRegisterEvent>(webSocketMessage.Message, SerializerSettings) is not { } registerEvent)
                    {
                        await user.Send(OpCodes.InvalidRequest, "Missing parameters [OP 5]");
                        break;
                    }

                    await DataBase.Register(registerEvent, user);
                    break;
                }

                case OpCodes.Identify when !user.IsIdentified:
                {
                    if (JsonConvert.DeserializeObject<LoginRegisterEvent>(webSocketMessage.Message, SerializerSettings) is not { } loginEvent)
                    {
                        await user.Send(OpCodes.InvalidRequest, "Missing parameters [OP 2]");
                        break;
                    }
                    
                    await DataBase.Login(loginEvent, user);
                    break;
                }

                //Actual commands
                case OpCodes.CreateServer:
                {
                    if (await DataBase.FindUserBySession(webSocketMessage.Session) is not { } foundUser)
                    {
                        await user.Send(OpCodes.InvalidRequest, "Invalid session");
                        break;
                    }

                    if (JsonConvert.DeserializeObject<ServerObject>(webSocketMessage.Message, SerializerSettings) is not { } serverObject)
                    {
                        await user.Send(OpCodes.InvalidRequest, "Missing parameters [OP 2]");
                        break;
                    }
                    
                    await DataBase.CreateServer(foundUser, serverObject);

                    await user.Send(Events.JoinedServer, JsonConvert.SerializeObject(serverObject));
                    break;
                }

                case OpCodes.JoinServer:
                {
                    if (await DataBase.FindUserBySession(webSocketMessage.Session) is not { } foundUser)
                    {
                        await user.Send(OpCodes.InvalidRequest, "Invalid session");
                        break;
                    }
                    
                    if (JsonConvert.DeserializeObject<JoinServer>(webSocketMessage.Message, SerializerSettings) is not { } joinObject)
                    {
                        await user.Send(OpCodes.InvalidRequest, "Missing parameters [OP 2]");
                        break;
                    }

                    await DataBase.JoinServer(joinObject, foundUser, user);
                    break;
                }

                case OpCodes.BanUser:
                {
                    if (await DataBase.FindUserBySession(webSocketMessage.Session) is not { } foundUser)
                    {
                        await user.Send(OpCodes.InvalidRequest, "Invalid session");
                        break;
                    }
                    
                    //Todo: Create object for banning and kicking
                    if (JsonConvert.DeserializeObject<JoinServer>(webSocketMessage.Message, SerializerSettings) is not { } joinObject)
                    {
                        await user.Send(OpCodes.InvalidRequest, "Missing parameters [OP 2]");
                        break;
                    }

                    break;
                }
            }
        }
    }
}