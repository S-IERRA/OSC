using System.Text.RegularExpressions;

using ChatShared.Json;
using ChatShared.Types;

using Microsoft.EntityFrameworkCore;

using Serilog;

namespace ChatServer.Handlers;

public partial record AccountService
{
    public async Task Register(LoginRegisterEvent registerEvent)
    {
        if (registerEvent is { Username: null } or { Email: null })
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

        string hashedPassword = Pbkdf2.CreateHash(registerEvent.Password);

        Context.Users.Add(new User()
        {
            Email = registerEvent.Email,
            Password = hashedPassword,
            Username = registerEvent.Username,
        });

        await Context.SaveChangesAsync();
        await SocketUser.Send(Events.Registered);

        Log.Information($"Ip {SocketUser.UnderSocket.RemoteEndPoint} registered");
    }

    public async Task Login(LoginRegisterEvent loginEvent)
    {
        try
        {
            User? user = await Context.Users
                .Where(u => u.Email == loginEvent.Email)
                .FirstOrDefaultAsync();

            Log.Debug("Called Login");

            if (user is null || !Pbkdf2.ValidatePassword(loginEvent.Password, user.Password))
            {
                await SocketUser.Send(OpCodes.InvalidRequest, ErrorMessages.InvalidUserOrPass);
                return;
            }

            string session = Guid.NewGuid().ToString();

            user.Session = session;
            SocketUser.SessionId = session;
            SocketUser.IsIdentified = true;

            await Context.SaveChangesAsync();
            await SocketUser.Send(Events.Identified, "TEST");

            //Log.Information($"User {user.Email} logged in");
        }
        catch(Exception e)
        {
            Log.Fatal(e, "Error while logging in");
        }
    }

    public async Task Login(string session)
    {
        if (SocketUser.IsIdentified)
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