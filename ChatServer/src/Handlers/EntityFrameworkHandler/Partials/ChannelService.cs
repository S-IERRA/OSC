using ChatServer.Extensions;
using ChatServer.Objects;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ChatServer.Handlers;

public class SendMessageEvent
{
    public string Content { get; set; }
    public int ChannelId { get; set; }
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
        try
        {
            if (!JsonHelper.TryDeserialize<RequestChannelMessages>(message, out var messageEvent))
            {
                await SocketUser.Send(OpCodes.InvalidRequest);
                return;
            }

            if (await Context.Channels.Where(x => x.Id == messageEvent.channel).Include(x => x.Messages)
                    .FirstOrDefaultAsync() is
                not { } channelSession)
            {
                await SocketUser.Send(OpCodes.InvalidRequest);

                return;
            }

            await SocketUser.Send(Events.MessagesRequested, channelSession.Messages);
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Error while getting channel messages");
        }
    }
    
    //Permissions vuln
    public async Task GetServerChannels(uint serverId)
    {
        if(await Context.Servers.Where(x => x.Id == serverId).Include(x=>x.Channels).FirstOrDefaultAsync() is
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
        try
        {
            if (!JsonHelper.TryDeserialize<SendMessageEvent>(message, out var messageEvent))
            {
                await SocketUser.Send(OpCodes.InvalidRequest);
                return;
            }
            
            Log.Debug(message);

            //get channel by id
            if (await Context.Channels.Where(x => x.Id == messageEvent.ChannelId).Include(x => x.Messages).FirstOrDefaultAsync() is
                not { } channel)
            {
                await SocketUser.Send(OpCodes.InvalidRequest);

                return;
            }

            Log.Debug("test2");

            //get member
            var member = await Context.Members.FirstOrDefaultAsync(x => x.Id == user.Id);
            //get server with id 1
            var server = await Context.Servers.FirstOrDefaultAsync(x => x.Id == 1);

            /*
            if (!sender.Permissions.HasFlag(channel.ViewPermission))
            {
                //Missing permissions
                return;
            }*/

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
        catch (Exception e)
        {
            Log.Fatal(e, "Error while sending message");
        }
    }
}