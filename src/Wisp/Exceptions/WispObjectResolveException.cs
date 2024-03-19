namespace Wisp;

[PublicAPI]
public class WispObjectResolveException : WispException
{
    public long Position { get; }
    public long Length { get; }

    internal WispObjectResolveException(CosParser parser, string message, Exception? innerException = null)
        : base($"{message}. At {parser.Position}/{parser.Length}", innerException)
    {
        Position = parser.Position;
        Length = parser.Length;
    }
}