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
    
    public static implicit operator BasicUser?(int x)
    {
        return Task.Run(async () => await SocketTest.DataBase.FindUserById(x)).Result;
    }
}

public class Subscriber : BasicUser
{
    public int Permissions { get; set; }
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
        return Task.Run(async () => await SocketTest.DataBase.FindUserById(x)).Result;
    }
    
    public static implicit operator UserProperties?(string x)
    {
        return Task.Run(async () => await SocketTest.DataBase.FindUserBySession(x)).Result;
    }
}