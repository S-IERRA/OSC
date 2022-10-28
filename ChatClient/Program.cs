// See https://aka.ms/new-console-template for more information

using ChatClient.Handlers;
using Newtonsoft.Json;

WebSocketHandler webSocket = new WebSocketHandler();

string sonData = JsonConvert.SerializeObject(new LoginRegisterEvent
{
    Password = "testPassword123",
    //Username = "Iskra",
    Email = "tesffft@mail.com"
});

await webSocket.Send(OpCodes.Identify, sonData);

Thread.Sleep(-1);