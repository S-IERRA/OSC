using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ChatServer.Handlers;
using Newtonsoft.Json;

namespace ChatServer.Objects;

[Flags]
public enum Permissions
{
    Member,
    CanKick,
    Administrator
}

[Obsolete]
public class BasicUser
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonProperty("userid")]     public int UserId                   { get; init; }
    
    [JsonProperty("created_at")] public DateTimeOffset CreationDate  { get; init; }
    [JsonProperty("display")]    public required string    Username  { get; set;  }
    [JsonProperty("display")]    public string?           AvatarUrl  { get; set;  }

    public static implicit operator int(BasicUser user) => user.UserId;

    public static implicit operator BasicUser?(int x) 
        => Task.Run(async () => await SocketServer.DataBase.FindUserById(x)).Result;
}

[Obsolete]
public class UserProperties : BasicUser
{
    [JsonProperty("email")]      public required string Email     { get; set;  }
    [JsonIgnore]                 public required string Password  { get; set;  }
    [JsonIgnore]                 public string?         Session   { get; set;  }
 
    [JsonProperty("friends")]    public List<UserProperties>  Friends   { get; set;  }
    [JsonProperty("Servers")]    public List<ServerObject>    Servers   { get; set;  }
    
    public static implicit operator UserProperties?(int x)
    {
        return Task.Run(async () => await SocketServer.DataBase.FindUserById(x)).Result;
    }
    
    public static implicit operator UserProperties?(string x)
    {
        return Task.Run(async () => await SocketServer.DataBase.FindUserBySession(x)).Result;
    }
}

[Obsolete]
public class ServerObject
{
    [JsonProperty("owner")] public required BasicUser Creator { get; set; }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonProperty("server_id")] public int Id { get; init; }

    [JsonProperty("name")] public required string Name { get; init; }
    [JsonProperty("icon")] public string?         Icon { get; init; }
    
    [JsonIgnore] public List<UserProperties>          Users { get; set; }
    [JsonIgnore] public List<ChannelObject>      Channels { get; set; }
    [JsonIgnore] public List<UserProperties>  BannedUsers { get; set; }

    public static implicit operator int(ServerObject serverObject) => serverObject.Id;
}

[Obsolete]
public class ServerMember
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonProperty("userid")] public int Id { get; init; }

    //public UserProperties User { get; set; }
    public ServerObject Server { get; set; }
    public Permissions Permissions { get; set; }
}