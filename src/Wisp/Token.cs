namespace Wisp;

public sealed class Token
{
    public TokenKind Kind { get; }
    public string? Text { get; }
    public byte[]? Lexeme { get; }

    public Token(TokenKind kind)
        : this(kind, null)
    {
    }

    public Token(TokenKind kind, string? text, byte[]? lexeme = null)
    {
        Kind = kind;
        Text = text;
        Lexeme = lexeme;
    }
}