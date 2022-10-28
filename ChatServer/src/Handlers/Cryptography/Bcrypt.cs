using BCrypt.Net;

namespace ChatServer.Handlers;

//Use this or fucking PBKDF2, PBKDF2 is less fucking brain-dead than this dog shit, what the fuck is "HashPassword" kys
public class Bcrypt
{
    static string Hash(string raw)
    {
        //BCrypt.HashPassword("");
        return "";
    }
}