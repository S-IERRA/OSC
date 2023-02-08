using ChatServer.Handlers;

/*
Factory factory = new();
EntityFramework ctx = factory.CreateDbContext();

await ctx.Database.EnsureDeletedAsync();
await ctx.Database.EnsureCreatedAsync();
*/
SocketServer a = new();
await a.Start();