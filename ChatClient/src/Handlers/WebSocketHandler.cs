using System;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using ChatServer.Extensions;

namespace ChatClient.Handlers
{
    public class CreateServerEvent
    { 
        public string Name { get; set; }
    }

    public class WebSocketHandler
    {
        private static readonly Socket Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        //Move this to a user config file
        public static string _userSession = "";

        private async void ReceiveMessages()
        {
            byte[] localBuffer = new byte[512];

            for (;;)
            {           
                using MemoryStream stream = new MemoryStream();
                
                do
                {
                    int received = await Client.ReceiveAsync(localBuffer, SocketFlags.None);
                    stream.Write(localBuffer, 0, received);
                } while (Client.Available > 0);

                string data = GZip.Decompress(stream.ToArray());
                if (!JsonHelper.TryDeserialize<WebSocketMessage>(data, out var socketMessage))
                {
                    Console.WriteLine("Invalid message");
                    continue;
                }

                HandleOpcode(socketMessage);
            }
        }
        
        private async Task HandleOpcode(WebSocketMessage messageEvent)
        {
            switch (messageEvent.OpCode)
            {
                case OpCodes.HeartBeat:
                {
                    await Send(OpCodes.HeartBeatAck);
                    Console.WriteLine("[HEARTBEAT] Sent ACK");
                    break;
                }

                case OpCodes.InvalidRequest:
                {
                    Console.WriteLine($"[ERROR] {messageEvent.OpCode} {messageEvent.Message}");
                    break;
                }

                case OpCodes.ConnectionClosed:
                {
                    Console.WriteLine("[ERROR] Connection closed");
                    break;
                }

                case OpCodes.Event:
                {
                    switch (messageEvent.EventType)
                    {
                        case Events.Identified:
                            if (!JsonHelper.TryDeserialize<User>(messageEvent.Message, out var userAccount))
                            {
                                Console.WriteLine("invalid json");
                                break;
                            }

                            _userSession = userAccount.Session;
                            Console.WriteLine("test");
                            break;

                        case Events.LoggedOut:
                            Console.WriteLine("Logged out");
                            break;

                        case Events.Registered:
                            Console.WriteLine("Registered");
                            break;
                    }

                    break;
                }
            }
        }

        public WebSocketHandler()
        {
            Client.DontFragment = true;
            Client.Connect(IPAddress.Parse("127.0.0.1"), 8787);
            ReceiveMessages();
        }

        private int index = 0;
        
        private async Task SendData(OpCodes opCode, Events? eventType = null, string? dataSerialized = default)
        {
            if (!Client.Connected)
                return;
            
            WebSocketMessage message = new WebSocketMessage(opCode, dataSerialized, eventType, _userSession);

            string messageSerialized = JsonSerializer.Serialize(message);
            byte[] dataCompressed = GZip.Compress(messageSerialized);

            await Client.SendAsync(dataCompressed, SocketFlags.None);
        }

        public async Task Send(OpCodes opCode) 
            => await SendData(opCode);
        
        public async Task Send(Events eventType)
            => await SendData(OpCodes.Event, eventType);
        
        public async Task Send(OpCodes opCode, Events eventType) 
            => await SendData(opCode, eventType);
        
        public async Task Send(OpCodes opCode, string? message) 
            => await SendData(opCode, null, message);

        public async Task Send(OpCodes opCode, object message)
        {
            string jsonMessage = JsonSerializer.Serialize(message);
            
            await SendData(opCode, null, jsonMessage);
        }
        
        public async Task Send(Events eventType, object message)
        { 
            string jsonMessage = JsonSerializer.Serialize(message);
            
            await SendData(OpCodes.Event, eventType, jsonMessage);
        }
    }
}