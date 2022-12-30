// See https://aka.ms/new-console-template for more information

using ChatClient.Handlers;
using System.Text.Json;
using ChatClient;
using ChatClient.Json;
using ChatClient.Types;

//Todo: Cachce system for msgs

WebSocketHandler webSocket = new();
string sonData = "";

for (;;)
{
    try
    {
        string[] input = Console.ReadLine()!.Split(':');
        string[] command = input[1].Split(',');

        Int32.TryParse(input[0], out int opcode);

        switch (opcode)
        {
            case 2:
                sonData = JsonSerializer.Serialize(new LoginRegisterEvent
                {
                    Password = command[0],
                    Email = command[1]
                });

                await webSocket.Send(OpCodes.Identify, sonData);
                break;

            case 5:
                sonData = JsonSerializer.Serialize(new LoginRegisterEvent
                {
                    Password = command[0],
                    Username = command[1],
                    Email = command[2]
                });

                await webSocket.Send(OpCodes.Register, sonData);
                break;

            //join server
            case 8:
                sonData = JsonSerializer.Serialize(new JoinServerEvent()
                {
                    InviteCode = command[0]
                });

                await webSocket.Send(OpCodes.JoinServer, sonData);
                break;

            case 6:
                sonData = JsonSerializer.Serialize(new SendMessageEvent()
                {
                    Content = command[0],
                    ChannelId = 1
                });
                
                await webSocket.Send(OpCodes.SendMessage, sonData);
                break;

            case 15:
                sonData = JsonSerializer.Serialize(new RequestChannelMessages()
                {
                    channel = Int32.Parse(command[0])
                });

                await webSocket.Send(OpCodes.RequestChannelMessages, sonData);
                break;

            case 7:
                sonData = JsonSerializer.Serialize(new CreateServerEvent()
                {
                    Name = "testServer"
                });

                await webSocket.Send(OpCodes.CreateServer, sonData);
                break;

            case 16:
                sonData = JsonSerializer.Serialize(new CreateInvite()
                {
                    ServerId = Int32.Parse(command[0]),
                    Invite = command[1]
                });

                await webSocket.Send(OpCodes.CreateServerInvite, sonData);
                break;
        }
    }
    catch (Exception e)
    {
        
    }
}