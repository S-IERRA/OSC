using ChatServer.Extensions;
using ChatServer.Objects;
using ChatShared.Types;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ChatServer.Handlers;

public class SendMessageEvent
{
    public string Content { get; set; }
    public int ChannelId { get; set; }
    public int ServerId{ get; set; }
}

public class RequestChannelMessages
{
    public int channel { get; set; }
}

public partial record AccountService
{
    //Permissions vuln
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

        if (await Context.Channels.Where(x => x.Id == messageEvent.channel).Include(x => x.Messages)
                .FirstOrDefaultAsync() is
            not { } channelSession)
        {
            await SocketUser.Send(OpCodes.InvalidRequest);

            return;
        }

        await SocketUser.Send(Events.MessagesRequested, channelSession.Messages);
    }

    //Permissions vuln
    public async Task GetServerChannels(uint serverId)
    {
        if (await Context.Servers.Where(x => x.Id == serverId).Include(x => x.Channels).FirstOrDefaultAsync() is
            not { } server)
        {
            await SocketUser.Send(OpCodes.InvalidRequest);

            return;
        }

        await SocketUser.Send(Events.ChannelsRequested, server.Channels);
    }

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

    public async Task SendMessage(string? message, User user)
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

        Log.Debug("test2");

        var member = await Context.Members.FirstOrDefaultAsync(x => x.Id == user.Id);
        var server = await Context.Servers.FirstOrDefaultAsync(x => x.Id == messageEvent.ServerId);
        
        if (!member.Permissions.HasFlag(channel.ViewPermission))
        {
            await SocketUser.Send(OpCodes.InvalidRequest);
            return;
        }

        Message newMessage = new()
        {
            Author = member,
            Channel = channel,
            Server = server,
            Content = messageEvent.Content
        };

        channel.Messages.Add(newMessage);

        await Context.SaveChangesAsync();

        Log.Information(
            $"Member {member.User.Username} sent a message in channel {channel.Name} in server {channel.Server.Name}");

    }
}