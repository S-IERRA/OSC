using ChatServer.Handlers;
using ChatServer.Objects;

Factory factory = new Factory();
EntityFramework2 ctx = factory.CreateDbContext();
await ctx.Database.EnsureDeletedAsync();
await ctx.Database.EnsureCreatedAsync();

await ctx.CreateUser();

Thread.Sleep(-1);