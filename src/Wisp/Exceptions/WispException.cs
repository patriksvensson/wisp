namespace Wisp;

[PublicAPI]
public class WispException : Exception
{
    internal WispException(string? message)
        : base(message)
    {
    }

    internal WispException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}