using System.Text.RegularExpressions;
using ChatServer.Objects;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Handlers;

public partial record AccountService
{
      public async Task Register(LoginRegisterEvent registerEvent)
    {
        if (registerEvent is { Username: null } or {Email: null})
        {
            await SocketUser.Send(OpCodes.InvalidRequest, "Fields are missing.");
            return;
        }

        if (!Regex.IsMatch(registerEvent.Email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$",
                RegexOptions.NonBacktracking | RegexOptions.Compiled)
            || registerEvent.Email.Length > 254)
        {
            await SocketUser.Send(OpCodes.InvalidRequest, "Invalid email.");
            return;
        }

        if (registerEvent.Password.Length is < 6 or > 27)
        {
            await SocketUser.Send(OpCodes.InvalidRequest, "Invalid password length.");
            return;
        }

        if (Context.Users.Any(user => user.Email == registerEvent.Email))
        {
            await SocketUser.Send(OpCodes.InvalidRequest, "Email already exists.");
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
    }
    
    public async Task Login(LoginRegisterEvent loginEvent)
    {
        if (await Context.Users.SingleOrDefaultAsync(x => x.Email == loginEvent.Email && x.Password == loginEvent.Password) 
            is not { } userSession)
        {
            await SocketUser.Send(OpCodes.InvalidRequest, "Invalid username or password");

            return;
        }

        userSession.Session = RandomImpl.RandomString(24);

        await Context.SaveChangesAsync();

        SocketUser.IsIdentified = true;

        await SocketUser.Send(Events.Identified, userSession);
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
    }

    public async Task LogOut(User user)
    {
        if (!SocketUser.IsIdentified)
            return;
        
        user.Session = null;

        await Context.SaveChangesAsync();

        SocketUser.IsIdentified = false;

        await SocketUser.Send(Events.LoggedOut);
    }
}