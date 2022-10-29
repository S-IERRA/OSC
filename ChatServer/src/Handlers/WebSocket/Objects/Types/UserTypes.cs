using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ChatServer.Handlers;
using Newtonsoft.Json;

namespace ChatServer.Objects;

public class BasicUser
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonProperty("userid")]     public int UserId                   { get; init; }
    [JsonProperty("created_at")] public DateTimeOffset CreationDate  { get; init; }
        
    [JsonProperty("display")]    public required string Username      { get; set;  }
    [JsonProperty("display")]    public string? PictureUrl            { get; set;  }

    public static implicit operator int(BasicUser user) => user.UserId;

    public static implicit operator BasicUser?(int x) 
        => Task.Run(async () => await SocketServer.DataBase.FindUserById(x)).Result;
}

public class UserProperties : BasicUser
{
    [JsonProperty("email")]      public required string Email     { get; set;  }
    [JsonIgnore]                 public required string Password  { get; set;  }
    [JsonIgnore]                 public string?         Session   { get; set;  }
 
    [JsonProperty("friends")]    public List<BasicUser>     Friends   { get; set;  }
    [JsonProperty("friends")]    public List<ServerObject>  Servers   { get; set;  }
    
    public static implicit operator UserProperties?(int x)
    {
        return Task.Run(async () => await SocketServer.DataBase.FindUserById(x)).Result;
    }
    
    public static implicit operator UserProperties?(string x)
    {
        return Task.Run(async () => await SocketServer.DataBase.FindUserBySession(x)).Result;
    }
}

[Flags]
public enum Permissions
{
    Member,
    CanKick,
    Administrator
}

public class Subscriber : BasicUser
{
    public Permissions Permissions { get; set; }
}