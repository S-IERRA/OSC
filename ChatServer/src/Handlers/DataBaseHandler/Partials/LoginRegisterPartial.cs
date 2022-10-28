using System.Text.RegularExpressions;
using ChatServer.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace ChatServer.Handlers;

public partial class TestOrm
{
    //Todo: prepare for websocket (remove optional)
    public async Task<EntityEntry<UserProperties>?> Register(LoginRegisterEvent registerEvent, SocketUser? socketUser = null)
    {
        if (registerEvent.Username is null || registerEvent.Email is null)
        {
            //await socketUser.Send(OpCodes.InvalidRequest, "Fields are missing.");
            return null;
        }

        if (!Regex.IsMatch(registerEvent.Email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$",
                RegexOptions.NonBacktracking | RegexOptions.Compiled)
            || registerEvent.Email.Length > 254)
        {
            //await socketUser.Send(OpCodes.InvalidRequest, "Invalid email.");
            return null;
        }

        if (registerEvent.Password.Length is < 6 or > 27)
        {
            //await socketUser.Send(OpCodes.InvalidRequest, "Invalid password.");
            return null;
        }

        if (Users.Any(user => user.Email == registerEvent.Email))
        {
            //await socketUser.Send(OpCodes.InvalidRequest, "Email already exists.");
            return null;
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
        return user;
    }

    //Todo: Add login via username and password
    //If sessions are changed, implement login via session
    public async Task Login(LoginRegisterEvent loginEvent, SocketUser user)
    {
        string password = loginEvent.Password;
        string? email = loginEvent.Email,
            username = loginEvent.Username;

        string login = email == "" ? username : email;

        //hash the password

        //Login via email / username
        if (await Users.SingleOrDefaultAsync(x => x.Email == email && x.Password == password) is not { } userSession)
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