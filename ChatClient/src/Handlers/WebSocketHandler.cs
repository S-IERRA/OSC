using System.Net;
using System.Net.Sockets;

using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using ChatShared;
using ChatShared.Types;

namespace ChatClient.Handlers
{
    public class WebSocketHandler
    {
        private static readonly Socket Client = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        private readonly Dictionary<uint, TaskCompletionSource<WebSocketMessage>> _replyTasks = new();

        //Move this to a user config file
        private static Guid _userSession { get; set; }
        private static uint PacketIndex = 1;

        private async void ReceiveMessages()
        {
            byte[] localBuffer = new byte[512];

            for (;;)
            {
                MemoryStream dataStream = new();

                do
                {
                    int received = await Client.ReceiveAsync(localBuffer, SocketFlags.None);
                    dataStream.Write(localBuffer, 0, received);
                } 
                while (Client.Available > 0);
                
                byte[] decompressedBytes = await GZip.Decompress(dataStream.ToArray());
                await dataStream.DisposeAsync();

                for (int totalRead = 0; decompressedBytes.Length - totalRead > 0;)
                {
                    uint replyId = GZip.Byte2UInt(decompressedBytes, totalRead + 4);
                    int length = GZip.Byte2Int(decompressedBytes, totalRead + 8);

                    string rawMessage = Encoding.UTF8.GetString(decompressedBytes, totalRead + 12, length);
                    totalRead += length + 12;
                    PacketIndex++;

                    if (!JsonHelper.TryDeserialize<WebSocketMessage>(rawMessage, out var socketMessage))
                    {
                        Console.WriteLine("Invalid message");
                        continue;
                    }
                    
                    if (_replyTasks.TryGetValue(replyId, out var replyTask))
                    {
                        _replyTasks.Remove(replyId);
                        replyTask.SetResult(socketMessage);
                    }

                    if (socketMessage.OpCode == OpCodes.HeartBeat) 
                        await Send(OpCodes.HeartBeatAck);

                    if(socketMessage.EventType is not null)
                        Console.WriteLine(socketMessage.Message);
                }
            }
        }

        public WebSocketHandler()
        {
            Client.DontFragment = true;
            
            //Resolve dns
            IPAddress addresses = Dns.GetHostAddresses("0.tcp.eu.ngrok.io")[0];
            IPEndPoint endPoint = new IPEndPoint(addresses, 11312);
            
            Client.Connect(endPoint);
            ReceiveMessages();
        }
        
        //todo: clean these up
        private async Task SendData(OpCodes opCode, Events? eventType = null, string? dataSerialized = default)
        {
            if (!Client.Connected)
                return;
            
            WebSocketMessage message = new(opCode, dataSerialized, eventType, _userSession);

            string messageSerialized = JsonSerializer.Serialize(message);
            byte[] dataCompressed = GZip.Compress(messageSerialized, PacketIndex);

            await Client.SendAsync(dataCompressed, SocketFlags.None);
        }
        
        private async Task<WebSocketMessage?> SendWithReply(OpCodes opCode, Events? eventType = null, string? dataSerialized = default)
        {
            if (!Client.Connected)
                return null;
            
            WebSocketMessage message = new(opCode, dataSerialized, eventType, _userSession);
            string socketMessageSerialized = JsonSerializer.Serialize(message);
            
            byte[] dataCompressed = GZip.Compress(socketMessageSerialized, PacketIndex);

            var replyTask = new TaskCompletionSource<WebSocketMessage>();
            _replyTasks[PacketIndex] = replyTask;
            
            await Client.SendAsync(dataCompressed, SocketFlags.None);

            return await replyTask.Task;
        }

        public async Task Send(OpCodes opCode) 
            => await SendData(opCode);
        
        public async Task Send(Events eventType)
            => await SendData(OpCodes.Event, eventType);
        
        public async Task Send(OpCodes opCode, Events eventType) 
            => await SendData(opCode, eventType);
        
        public async Task Send(OpCodes opCode, string? message) 
            => await SendData(opCode, null, message);

        public async Task<WebSocketMessage?> SendReply(OpCodes opCode) 
            => await SendWithReply(opCode);
        
        public async Task<WebSocketMessage?> SendReply(Events eventType)
            => await SendWithReply(OpCodes.Event, eventType);
        
        public async Task<WebSocketMessage?> SendReply(OpCodes opCode, Events eventType) 
            => await SendWithReply(opCode, eventType);
        
        public async Task<WebSocketMessage?> SendReply(OpCodes opCode, string? message) 
            => await SendWithReply(opCode, null, message);

        public async Task<WebSocketMessage?> SendReply(OpCodes opCode, object message)
        {
            string jsonMessage = JsonSerializer.Serialize(message);
            
            return await SendWithReply(opCode, null, jsonMessage);
        }
        
        public async Task<WebSocketMessage?> SendReply(Events eventType, object message)
        { 
            string jsonMessage = JsonSerializer.Serialize(message);
            
            return await SendWithReply(OpCodes.Event, eventType, jsonMessage);
        }
        
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