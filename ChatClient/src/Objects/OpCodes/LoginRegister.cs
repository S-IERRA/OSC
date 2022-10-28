using Newtonsoft.Json;

namespace ChatClient.Handlers;

public class LoginRegisterEvent
{
    [JsonProperty("password")] public required string Password  { get; init; }

    [JsonProperty("username")] public string? Username          { get; init; }
    [JsonProperty("email")]    public string? Email             { get; init; }
}