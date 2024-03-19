namespace Wisp;

[PublicAPI]
public class WispLexerException : WispException
{
    public long Position { get; }
    public long Length { get; }

    internal WispLexerException(CosLexer lexer, string message, Exception? innerException = null)
        : base($"{message}. At {lexer.Position}/{lexer.Length}", innerException)
    {
        Position = lexer.Position;
        Length = lexer.Length;
    }
}