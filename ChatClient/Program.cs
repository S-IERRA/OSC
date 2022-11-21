// See https://aka.ms/new-console-template for more information

using ChatClient.Handlers;
using System.Text.Json;

//Cachce system for msgs

WebSocketHandler webSocket = new WebSocketHandler();

string sonData = JsonSerializer.Serialize(new LoginRegisterEvent
{
    Password = "testPassword123",
    //Username = "Iskra",
    Email = "tesfgft@mail.com"
});

await webSocket.Send(OpCodes.Identify, sonData);

string jsonData = JsonSerializer.Serialize(new CreateServerEvent
{
    Name = "TestServer",
    /*Icon = "https://cdn.discordapp.com/icons/123456789012345678/123456789012345678.png",*/
});

await webSocket.Send(OpCodes.CreateServer, jsonData);

Thread.Sleep(-1);


//Test commit