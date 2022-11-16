// See https://aka.ms/new-console-template for more information

using ChatClient.Handlers;
using Newtonsoft.Json;

//Cachce system for msgs

WebSocketHandler webSocket = new WebSocketHandler();

string sonData = JsonConvert.SerializeObject(new LoginRegisterEvent
{
    Password = "testPassword123",
    //Username = "Iskra",
    Email = "tesffft@mail.com"
});

await webSocket.Send(OpCodes.Identify, sonData);

Thread.Sleep(-1);


//Test commit