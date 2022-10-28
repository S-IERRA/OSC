namespace ChatServer.Handlers;

public class RandomImpl
{
    private static readonly Random Random = new Random();
    private const string Alphabet = "ABCDEFGHIKLMNOPQRSTVXYZabcdefghiklmnopqrstvxyz0123456789";

    private static int RandomNumber => Random.Next(0, 56);
    private static char  RandomChar => Alphabet[RandomNumber];

    public static string RandomString(uint length)
    {
        string returnString = "";
        for (uint i = 0; i < length; i++)
            returnString += RandomChar;

        return returnString;
    }
}