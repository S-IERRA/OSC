using System.Runtime.CompilerServices;

namespace ChatServer.Extensions;

public static partial class Extensions
{
    public static TaskAwaiter GetAwaiter(this int number) 
        => Task.Delay(number * 1000).GetAwaiter();
    
    //This can cause problems due to the uint -> int conversion, but it shouldn't as this should be very rarely used for awaiting ms
    //await 2000u
    public static TaskAwaiter GetAwaiter(this uint number) 
        => Task.Delay((int)number).GetAwaiter();
}