using System.Net;
using System.Text;
using ChatServer.Extensions;
using ChatServer.Objects;
using ChatShared;
using ChatShared.Json;
using ChatShared.Types;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ChatServer.Handlers;

public partial record AccountService
{
    //Todo: Permissions vuln
    public async Task GetChannelMessages(string? message)
    {
        if (!JsonHelper.TryDeserialize<RequestChannelMessages>(message, out var messageEvent))
        {
            await SocketUser.Send(OpCodes.InvalidRequest);
            return;
        }

        /* This code is vulnerable to any user being able to see any channel's messages
        var member = await Context.Members.FirstOrDefaultAsync(x => x.Id == messageEvent.);
        if (!member.Permissions.HasFlag(channel.ViewPermission))
        {
            await SocketUser.Send(OpCodes.InvalidRequest);
            return;
        }*/

        if (await Context.Channels.Where(x => x.Id == messageEvent.ChannelId).Include(x => x.Messages)
                .FirstOrDefaultAsync() is
            not { } channelSession)
        {
            await SocketUser.Send(OpCodes.InvalidRequest);

            return;
        }

        await SocketUser.Send(Events.MessagesRequested, channelSession.Messages);
    }

    //Todo: Permissions vuln
    public async Task GetServerChannels(Guid serverId)
    {
        if (await Context.Servers.Where(x => x.Id == serverId).Include(x => x.Channels).FirstOrDefaultAsync() is
            not { } server)
        {
            await SocketUser.Send(OpCodes.InvalidRequest);

            return;
        }

        await SocketUser.Send(Events.ChannelsRequested, server.Channels);
    }

    //Todo: lazily load messages
    public async Task GetChannelMessagesLazy(Channel channel, Range range)
    {
        /* why the fuck is this purple
        if (await Channels.Where(x => x.Id == channel.Id).Include(x => x.Messages).FirstOrDefaultAsync() is
            not { } channelSession)
        {
            await socketUser.Send(OpCodes.InvalidRequest);

            return;
        }

        await socketUser.Send(OpCodes.RequestChannelMessages, JsonConvert.SerializeObject(channelSession.Messages));
        */
    }

    //Todo: Messages have to be pushed to subsccribers
    public async Task SendMessage(string? message, User user)
    {
        try
        {
            if (!JsonHelper.TryDeserialize<SendMessageEvent>(message, out var messageEvent))
            {
                await SocketUser.Send(OpCodes.InvalidRequest);
                return;
            }

            //get channel by id
            if (await Context.Channels.Where(x => x.Id == messageEvent.ChannelId).Include(x => x.Messages)
                    .FirstOrDefaultAsync() is
                not { } channel)
            {
                await SocketUser.Send(OpCodes.InvalidRequest);

                return;
            }

            //Todo: null checks
            var member = await Context.Members.FirstOrDefaultAsync(x => x.UserId == user.Id);

            if (!member.Permissions.HasFlag(channel.ViewPermissions))
            {
                await SocketUser.Send(OpCodes.InvalidRequest);
                return;
            }

            Message newMessage = new()
            {
                Id = Guid.NewGuid(),
                AuthorId = member.UserId,
                ChannelId = channel.Id,
                ServerId = channel.ServerId,
                Content = messageEvent.Content
            };

            Context.Messages.Add(newMessage);
            await Context.SaveChangesAsync();

            //Null checks
            var subscribers = Context.Subscribers.Where(x => x.ServerId == channel.ServerId);
            foreach (var subscriber in subscribers)
                await SocketUser.UnderSocket.SendToAsync("test123"u8.ToArray(), new IPEndPoint(new IPAddress(subscriber.AddressBytes), subscriber.Port)); //Write a wrapper, temp method

            Log.Information("Member {Username} sent a message in channel {ChannelName} in server {ServerName}", member.User.Username, channel.Name, channel.Server.Name);

        }
        catch (Exception e)
        {
            Log.Fatal($"{e}");
        }
    }
}