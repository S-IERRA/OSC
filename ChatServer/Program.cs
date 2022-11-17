using ChatServer.Handlers;

Factory factory = new Factory();
EntityFramework2 ctx = factory.CreateDbContext();

await ctx.Database.EnsureDeletedAsync();
await ctx.Database.EnsureCreatedAsync();

//AccountService ac = new AccountService();

Thread.Sleep(-1);