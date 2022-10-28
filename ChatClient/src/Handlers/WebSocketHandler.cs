using System;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ChatClient.Handlers
{
    public class WebSocketHandler
    {
        private static readonly Socket Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

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

                string data = GZip.Decompress(stream);
                MessageEvent? message = JsonConvert.DeserializeObject<MessageEvent>(data);
                
                if (message is null)
                {
                    Console.WriteLine("[CLIENT] Invalid Message");
                    return;
                }

                HandleOpcode(message);
            }
        }
        
        private async Task HandleOpcode(MessageEvent messageEvent)
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
                            Console.WriteLine("Identified");
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
            Client.Connect(IPAddress.Parse("127.0.0.1"), 8787);
            ReceiveMessages();
        }

        public async Task Send(OpCodes opCode, string data = "", Events? eventType = Events.Null)
        {
            string serialized = JsonConvert.SerializeObject(new MessageEvent(opCode, data, eventType));

            byte[] dataCompressed = GZip.Compress(serialized);
            
            await Client.SendAsync(dataCompressed, SocketFlags.None);
        }
        
        public async Task Send(Events eventName, string data = "")
        {
            string serialized = JsonConvert.SerializeObject(new MessageEvent(OpCodes.Event, data, eventName));

            byte[] dataCompressed = GZip.Compress(serialized);
            
            await Client.SendAsync(dataCompressed, SocketFlags.None);
        }
    }
}