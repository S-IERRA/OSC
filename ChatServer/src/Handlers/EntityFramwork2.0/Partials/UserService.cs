using ChatShared.DTos;
using ChatShared.Json;
using ChatShared.Types;
using FluentValidation.Results;

using Microsoft.EntityFrameworkCore;

using Serilog;

namespace ChatServer.Handlers;

public partial record AccountService
{
    public async Task Register(LoginRegisterEvent registerEvent)
    {
        LoginRegisterEventValidator validator = new(Context);
        ValidationResult result = await validator.ValidateAsync(registerEvent);

        if (!result.IsValid)
        {
			await SocketUser.Send(OpCodes.InvalidRequest, result.Errors);
			return;
        }

		string hashedPassword = Pbkdf2.CreateHash(registerEvent.Password);

        Context.Users.Add(new User()
        {
            Id = Guid.NewGuid(),
            Email = registerEvent.Email!,
            Password = hashedPassword,
            Username = registerEvent.Username!,
        });

        await Context.SaveChangesAsync();
        await SocketUser.Send(Events.Registered);

        Log.Information($"Ip {SocketUser.UnderSocket.RemoteEndPoint} registered");
    }

    public async Task Login(LoginRegisterEvent loginEvent)
    {
        User? user = await Context.Users
            .Where(u => u.Email == loginEvent.Email)
            .FirstOrDefaultAsync();

        if (user is null || !Pbkdf2.ValidatePassword(loginEvent.Password, user.Password))
        {
            await SocketUser.Send(OpCodes.InvalidRequest, ErrorMessages.InvalidUserOrPass);
            return;
        }

        Guid session = Guid.NewGuid();

        user.Session = session;
        SocketUser.SessionId = session;
        SocketUser.IsIdentified = true;

        await Context.SaveChangesAsync();
        await SocketUser.Send(Events.Identified, "test"); //Todo: Convert to DTo with Auto mapper

        Log.Information($"User {user.Email} logged in");
    }

    public async Task Login(Guid session)
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

        var userDTo = (UserLoggedIn)userSession;

        await SocketUser.Send(Events.Identified, userDTo);

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