namespace ChatServer.Handlers;

public partial record AccountService
{
    public async Task GetChannelMessages(Channel channel, Range range)
    {
        /*
        if (await Channels.Where(x => x.Id == channel.Id).Include(x => x.Messages).FirstOrDefaultAsync() is
            not { } channelSession)
        {
            await socketUser.Send(OpCodes.InvalidRequest);

            return;
        }

        await socketUser.Send(OpCodes.RequestChannelMessages, JsonConvert.SerializeObject(channelSession.Messages));
        */
        
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

    public async Task SendMessage()
    {
        
    }
}