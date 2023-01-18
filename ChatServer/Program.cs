using ChatServer.Rewrite;
using Microsoft.EntityFrameworkCore;

//SocketServer2 a = new SocketServer2();
//await a.Start();

Factory2 factory = new();
EntityFramework3 ctx = factory.CreateDbContext();

await ctx.Database.EnsureDeletedAsync();
await ctx.Database.EnsureCreatedAsync();

Guid userId = Guid.NewGuid();
Guid serverId = Guid.NewGuid();

ctx.Users.Add(new User
{
    Email = "test@gmail.com",
    Id = userId,
    Password = "test123",
    Username = "S-IERRA"
});

ctx.Servers.Add(new Server
{
    Id = serverId,
    Name = "Test",
    OwnerId = userId
});

ctx.Members.Add(new Member
{
    UserId = userId,
    ServerId = serverId
});

await ctx.SaveChangesAsync();

Thread.Sleep(-1);

//AccountService ac = new AccountService();
//
/*static string Pad(byte b)
{
    return Convert.ToString(b, 2).PadLeft(8, '0');
}

byte[] compressed = GZip.Compress(1 , "test");
byte[] decompressed = GZip.Decompress(compressed);
Console.WriteLine(decompressed.Length);

int id = GZip.GetInt(decompressed);
int replyId = GZip.GetInt(decompressed, 4);
int messageLength = GZip.GetInt(decompressed, 8);

string message = Encoding.UTF8.GetString(decompressed, 12, messageLength);

Console.WriteLine($"id: {id}, replyId: {replyId}, messageLength: {messageLength}, message: {message}");

Console.WriteLine();

 //  4     4       4        ?
    //[ID][REPLYID][LENGTH](MESSAGE)

record struct Packet(int Id, int ReplyId, int Length, byte[] Message);*/

/*
SocketServer2 server = new();
await server.Start();
*/
//Thread.Sleep(-1);