// See https://aka.ms/new-console-template for more information

using ChatClient.Handlers;
using System.Text.Json;

using ChatShared.Json;
using ChatShared.Types;

//Todo: Cachce system for msgs
//The entire client side is temporary as I have not yet have had time to create a client side UI, the actual code is on the server side.

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

                var reply = await webSocket.SendReply(OpCodes.Identify, sonData);
                Console.WriteLine(reply.Message);
                break;

            case 5:
                sonData = JsonSerializer.Serialize(new LoginRegisterEvent
                {
                    Password = command[0],
                    Username = command[1],
                    Email = command[2]
                });

                reply = await webSocket.SendReply(OpCodes.Register, sonData);
                Console.WriteLine(reply.Message);
                break;
            
            //join server
            case 8:
                sonData = JsonSerializer.Serialize(new JoinServerEvent(command[0]));

                reply = await webSocket.SendReply(OpCodes.JoinServer, sonData);
                Console.WriteLine(reply.Message);
                break;

            case 6:
                sonData = JsonSerializer.Serialize(new SendMessageEvent(command[0], Guid.Parse(command[1]), Guid.Parse(command[2])));

                reply = await webSocket.SendReply(OpCodes.SendMessage, sonData);
                Console.WriteLine(reply.Message);           
                break;

            case 15:
                sonData = JsonSerializer.Serialize(new RequestChannelMessages(Guid.Parse(command[0])));

                reply = await webSocket.SendReply(OpCodes.RequestChannelMessages, sonData);
                Console.WriteLine(reply.Message);       
                break;

            case 7:
                sonData = JsonSerializer.Serialize(new CreateServerEvent(command[0]));

                reply = await webSocket.SendReply(OpCodes.CreateServer, sonData);
                Console.WriteLine(reply.Message);
                break;

            case 16:
                sonData = JsonSerializer.Serialize(new CreateInvite(Guid.Parse(command[0]), command[1]));

                reply = await webSocket.SendReply(OpCodes.CreateServerInvite, sonData);
                Console.WriteLine(reply.Message);           
                break;

            case 17:
				sonData = JsonSerializer.Serialize(new ServerEvent(Guid.Parse(command[0])));

                reply = await webSocket.SendReply(OpCodes.SubscribeServerMessages, sonData);
                Console.WriteLine(reply.Message);		
                break;
		}
    }
    catch (Exception e)
    {
        
    }
}