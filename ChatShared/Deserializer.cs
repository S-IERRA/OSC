using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace ChatShared;

public class SnakeCase : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        StringBuilder returnValue = new();

        for (var index = 0; index < name.Length; index++)
        {
            char currentChar = name[index];
            
            switch (currentChar)
            {
                case >= 'A' and <= 'Z':
                {
                    if (index != 0 && name[index - 1] is >= 'a' and <= 'z')
                        returnValue.Append('_');

                    returnValue.Append(Char.ToLower(currentChar));
                    continue;
                }
                default:
                    returnValue.Append(currentChar);
                    break;
            }
        }

        return returnValue.ToString();
    }
}

//Optimise this
public static class JsonHelper
{
    public static bool TryDeserialize<TClass>(string? message, [NotNullWhen(true)] out TClass? result)
    {
        result = default;

        if (message is null)
            return false;

        try
        {
            result = JsonSerializer.Deserialize<TClass>(message)!;
            return true;
        }
        catch(Exception e)
        {
            return false;
        }
    }

    public static bool TryDeserializeTo<TClass>(this string message, out TClass? result)
        => TryDeserialize(message, out result);
}