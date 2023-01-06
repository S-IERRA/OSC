namespace ChatShared.Json;

public class LoginRegisterEvent
{
     public required string Password  { get; init; }

     public string? Username          { get; init; }
     public string? Email             { get; init; }
}

public class IdentifiedEventShared
{
     public required string Session { get; set; }
}