// See https://aka.ms/new-console-template for more information

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using ChatServer.Extensions;
using ChatServer.Handlers;
using ChatServer.Objects;
using Microsoft.EntityFrameworkCore;

await using var ctx = new EntityFrameworkOrm();

/*
await ctx.Database.EnsureDeletedAsync();
await ctx.Database.EnsureCreatedAsync();

async Task CreateUser(int i)
{
    var a = await ctx.Register(new LoginRegisterEvent
    {
        Email = $"taesfft{i}@gmail.com",
        Password = "test1234",
        Username = $"user{i}"
    });   
}

foreach (int i in 3)
    await CreateUser(i);

//SERVER 1
await ctx.CreateServer(1);

foreach (int i in 3)
{
    await ctx.JoinServer(1, i);
}

//SERVER 2
await ctx.CreateServer(1);

foreach (int i in 3)
{
    await ctx.JoinServer(2, i);
}

Console.WriteLine("DONE!");

//Find the servers

var server = await ctx.FindServerById(1);
foreach (var s in server.Users)
{
    Console.WriteLine(s.Username + ' ' + 1);
}

Console.WriteLine();

var server2 = await ctx.FindServerById(2);
foreach (var s in server2.Users)
{
    Console.WriteLine(s.Username + ' ' + 2);
}

Console.WriteLine("DONE!");*/

Thread.Sleep(-1);