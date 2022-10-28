using Newtonsoft.Json;

namespace ChatServer.Objects;

public class Identified
{
    [JsonProperty("userid")]     int UserId  {get;set;}
    [JsonProperty("created_at")] DateTimeOffset CreationDate {get;set;}
        
    [JsonProperty("email")]      string Email{get;set;} 
    [JsonProperty("display")]    string Username {get;set;}
    //[JsonProperty("friends")]    public UserProperties? Friends            { get; set;  }
    [JsonProperty("subscribed")] ulong[]? SubscribedChannels {get;set;}
}