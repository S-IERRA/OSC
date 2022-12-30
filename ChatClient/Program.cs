// See https://aka.ms/new-console-template for more information

using ChatClient.Handlers;
using System.Text.Json;
using ChatClient;

//Cachce system for msgs

WebSocketHandler webSocket = new WebSocketHandler();
string sonData = "";

for (;;)
{
    try
    {
        string[] input = Console.ReadLine()!.Split(':');
        string[] command = input[1].Split(',');

        Int32.TryParse(input[0], out int opcode);

        switch (opcode)
        {
            case 2:
                sonData = JsonSerializer.Serialize(new LoginRegisterEvent
                {
                    Password = command[0],
                    Email = command[1]
                });

                await webSocket.Send(OpCodes.Identify, sonData);
                break;

            case 5:
                sonData = JsonSerializer.Serialize(new LoginRegisterEvent
                {
                    Password = command[0],
                    Username = command[1],
                    Email = command[2]
                });

                await webSocket.Send(OpCodes.Register, sonData);
                break;

            //join server
            case 8:
                sonData = JsonSerializer.Serialize(new JoinServerEvent()
                {
                    InviteCode = command[0]
                });

                await webSocket.Send(OpCodes.JoinServer, sonData);
                break;

            case 6:
                sonData = JsonSerializer.Serialize(new SendMessageEvent()
                {
                    Content = command[0],
                    ChannelId = 1
                });
                
                await webSocket.Send(OpCodes.SendMessage, sonData);
                break;

            case 15:
                sonData = JsonSerializer.Serialize(new RequestChannelMessages()
                {
                    channel = Int32.Parse(command[0])
                });

                await webSocket.Send(OpCodes.RequestChannelMessages, sonData);
                break;

            case 7:
                sonData = JsonSerializer.Serialize(new CreateServerEvent()
                {
                    Name = "testServer"
                });

                await webSocket.Send(OpCodes.CreateServer, sonData);
                break;

            case 16:
                sonData = JsonSerializer.Serialize(new CreateInvite()
                {
                    ServerId = Int32.Parse(command[0]),
                    Invite = command[1]
                });

                await webSocket.Send(OpCodes.CreateServerInvite, sonData);
                break;
        }
    }
    catch (Exception e)
    {
        
    }
}

public class CreateInvite
{
    public int ServerId { get; set; }
    public string Invite { get; set; }
}

public class SendMessageEvent
{
    public string Content { get; set; }
    public int ChannelId { get; set; }
}

public class RequestChannelMessages
{
    public int channel { get; set; }
}

public class Member
{
    public int Id { get; set; }

    private int userId;
    private int serverId;

    public Permissions Permissions { get; set; }
    public DateTime Joined { get; set; } = DateTime.UtcNow;

    public Server Server { get; set; }
    public User User { get; set; }

    public virtual ICollection<Role> Roles { get; set; }
}

public class Channel
{
    public int Id { get; set; }

    public required string Name { get; set; }
    public required Permissions ViewPermission { get; set; }

    public required Server Server { get; set; }

    public virtual List<Message> Messages { get; set; }
}

public class Message
{
    public int Id { get; set; }

    public required Member Author { get; set; }
    public required Server Server { get; set; }
    public Channel? Channel { get; set; }
    public required string Content { get; set; }

    public DateTime Sent { get; set; }
}

[Flags]
public enum Permissions
{
    Member,
    Administrator,
    CanKick,
    CanBan,
    CanMute,
}

public class Server
{
    public int Id { get; set; }
    private int _ownerId;

    public required User Owner { get; set; }
    public required string Name { get; set; }

    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Banner { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Member> Members { get; set; }
    public virtual ICollection<Role> Roles { get; set; }
    public virtual ICollection<Channel> Channels { get; set; }
    public virtual ICollection<Invite> InviteCodes { get; set; }
}

public class Role
{
    public int Id { get; set; }

    private int userId;
    private int serverId;

    public required Server Server { get; set; }
    public Member User { get; set; }

    public required string Name { get; set; }
    public required int Color { get; set; } //Hex

    public required Permissions Permissions { get; set; }
}

public class Invite
{
    public int Id { get; set; }

    public int ServerId { get; set; }
    public string InviteCode { get; set; }
}