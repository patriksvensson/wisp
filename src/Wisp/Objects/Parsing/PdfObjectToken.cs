namespace Wisp;

internal sealed class PdfObjectToken
{
    public PdfObjectTokenKind Kind { get; }
    public string? Text { get; }
    public byte[]? Lexeme { get; }

    public PdfObjectToken(PdfObjectTokenKind kind)
        : this(kind, null)
    {
    }

    public PdfObjectToken(PdfObjectTokenKind kind, string? text, byte[]? lexeme = null)
    {
        Kind = kind;
        Text = text;
        Lexeme = lexeme;
    }
}