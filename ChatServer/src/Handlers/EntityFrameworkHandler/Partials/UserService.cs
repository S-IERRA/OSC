using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using ChatServer.Objects;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ChatServer.Handlers;

public partial record AccountService
{
    public async Task Register(LoginRegisterEvent registerEvent)
    {
        if (registerEvent is { Username: null } or {Email: null})
        {
            await SocketUser.Send(OpCodes.InvalidRequest, ErrorMessages.MissingFields);
            return;
        }

        if (!Regex.IsMatch(registerEvent.Email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$",
                RegexOptions.NonBacktracking | RegexOptions.Compiled)
            || registerEvent.Email.Length > 254)
        {
            await SocketUser.Send(OpCodes.InvalidRequest, ErrorMessages.InvalidEmail);
            return;
        }

        if (registerEvent.Password.Length is < 6 or > 27)
        {
            await SocketUser.Send(OpCodes.InvalidRequest, ErrorMessages.InvalidPasswordLength);
            return;
        }

        if (Context.Users.Any(user => user.Email == registerEvent.Email))
        {
            await SocketUser.Send(OpCodes.InvalidRequest, ErrorMessages.EmailAlreadyExists);
            return;
        }

        Context.Users.Add(new User()
        {
            Email = registerEvent.Email,
            Password = registerEvent.Password,
            Username = registerEvent.Username,
        });

        await Context.SaveChangesAsync();
        await SocketUser.Send(Events.Registered);
        
        Log.Information($"Ip {SocketUser.UnderSocket.RemoteEndPoint} registered");
    }

    public async Task Login(LoginRegisterEvent loginEvent)
    {
        if (await Context.Users.SingleOrDefaultAsync(x => x.Email == loginEvent.Email && x.Password == loginEvent.Password) 
            is not { } userSession)
        {
            await SocketUser.Send(OpCodes.InvalidRequest, ErrorMessages.InvalidUserOrPass);

            return;
        }

        string session = Guid.NewGuid().ToString();

        userSession.Session = session;
        SocketUser.SessionId = session;
        SocketUser.IsIdentified = true;

        await Context.SaveChangesAsync();
        await SocketUser.Send(Events.Identified, userSession);
        
        Log.Information($"User {userSession.Email} Logged in");
    }

    public async Task Login(string session)
    {
        if(SocketUser.IsIdentified)
            return;

        if (await Context.Users.SingleOrDefaultAsync(x => x.Session == session) is
            not { } userSession)
        {
            await SocketUser.Send(OpCodes.InvalidRequest);

            return;
        }
        
        SocketUser.IsIdentified = true;

        await SocketUser.Send(Events.Identified, userSession);
        
        Log.Information($"Ip {SocketUser.UnderSocket.RemoteEndPoint} Logged into account {userSession.Email}");
    }

    public async Task LogOut()
    {
        if (!SocketUser.IsIdentified)
            return;
        
        SocketUser.SessionId = null;
        SocketUser.IsIdentified = false;

        await Context.SaveChangesAsync();
        await SocketUser.Send(Events.LoggedOut);
        
        Log.Information($"Ip {SocketUser.UnderSocket.RemoteEndPoint} Logged out");
    }
}