using System.Text.RegularExpressions;
using ChatServer.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace ChatServer.Handlers;

public partial class EntityFrameworkOrm
{
    public async Task Register(LoginRegisterEvent registerEvent, SocketUser? socketUser = null)
    {
        if (registerEvent.Username is null || registerEvent.Email is null)
        {
            await socketUser.Send(OpCodes.InvalidRequest, "Fields are missing.");
            return;
        }

        if (!Regex.IsMatch(registerEvent.Email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$",
                RegexOptions.NonBacktracking | RegexOptions.Compiled)
            || registerEvent.Email.Length > 254)
        {
            await socketUser.Send(OpCodes.InvalidRequest, "Invalid email.");
            return;
        }

        if (registerEvent.Password.Length is < 6 or > 27)
        {
            await socketUser.Send(OpCodes.InvalidRequest, "Invalid password.");
            return;
        }

        if (Users.Any(user => user.Email == registerEvent.Email))
        {
            await socketUser.Send(OpCodes.InvalidRequest, "Email already exists.");
            return;
        }

        EntityEntry<UserProperties> user = Users.Add(new UserProperties
        {
            Email = registerEvent.Email,
            Password = registerEvent.Password,
            Username = registerEvent.Username,
            CreationDate = DateTimeOffset.UtcNow
        });

        await SaveChangesAsync();

        //await socketUser.Send(Events.Registered);
    }

    //Todo: If sessions are changed, implement login via session
    //10/29/22 - On a further review, if a token still exists and isn't invalidated, the client should still technically be logged in either way.
    public async Task Login(LoginRegisterEvent loginEvent, SocketUser user)
    {
        if (await Users.SingleOrDefaultAsync(x => x.Email == loginEvent.Email && x.Password == loginEvent.Password) is not { } userSession)
        {
            await user.Send(OpCodes.InvalidRequest, "Invalid username or password");

            return;
        }

        userSession.Session = RandomImpl.RandomString(24);

        await SaveChangesAsync();

        user.IsIdentified = true;

        await user.Send(Events.Identified, JsonConvert.SerializeObject(userSession));
    }

    public async Task LogOut(SocketUser user)
    {
        //invalidate session
        if (await FindUserBySession(user.SessionId) is { } userSession)
        {
            userSession.Session = null;

            await SaveChangesAsync();

            await user.Send(Events.LoggedOut);
            user.IsIdentified = false;
        }

        await user.Send(OpCodes.InvalidRequest, "Session not found");
    }
}