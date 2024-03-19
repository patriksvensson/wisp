namespace Wisp;

[PublicAPI]
public class WispParserException : WispException
{
    public long Position { get; }
    public long Length { get; }

    internal WispParserException(CosParser parser, string message, Exception? innerException = null)
        : base($"{message}. At {parser.Position}/{parser.Length}", innerException)
    {
        Position = parser.Position;
        Length = parser.Length;
    }
}