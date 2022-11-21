using ChatServer.Handlers;

/*
Factory factory = new Factory();
EntityFramework2 ctx = factory.CreateDbContext();

await ctx.Database.EnsureDeletedAsync();
await ctx.Database.EnsureCreatedAsync();*/

//AccountService ac = new AccountService();

SocketServer2 server = new();
await server.Start();

Thread.Sleep(-1);