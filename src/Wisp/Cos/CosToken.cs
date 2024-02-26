namespace Wisp.Cos;

public sealed class CosToken
{
    public CosTokenKind Kind { get; }
    public string? Text { get; }
    public byte[]? Lexeme { get; }

    public CosToken(CosTokenKind kind, string? text = null, byte[]? lexeme = null)
    {
        Kind = kind;
        Text = text;
        Lexeme = lexeme;
    }
}

public static class CosTokenExtensions
{
    public static int ParseInteger(this CosToken token)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (token.Kind != CosTokenKind.Integer)
        {
            throw new InvalidOperationException("Cannot parse token since it's not an integer.");
        }

        return token.Text == null
            ? 0
            : int.Parse(token.Text, CultureInfo.InvariantCulture);
    }

    public static double ParseReal(this CosToken token)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (token.Kind != CosTokenKind.Real)
        {
            throw new InvalidOperationException("Cannot parse token since it's not a real number.");
        }

        if (token.Text == null)
        {
            return 0;
        }

        return double.Parse(token.Text, CultureInfo.InvariantCulture);
    }
}