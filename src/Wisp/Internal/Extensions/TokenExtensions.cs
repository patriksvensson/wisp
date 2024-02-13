namespace Wisp.Internal;

internal static class TokenExtensions
{
    public static int ParseInteger(this PdfObjectToken token)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        if (token.Kind != PdfObjectTokenKind.Integer)
        {
            throw new InvalidOperationException("Cannot parse token since it's not an integer.");
        }

        if (token.Text == null)
        {
            return 0;
        }

        return int.Parse(token.Text, CultureInfo.InvariantCulture);
    }

    public static double ParseReal(this PdfObjectToken token)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        if (token.Kind != PdfObjectTokenKind.Real)
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