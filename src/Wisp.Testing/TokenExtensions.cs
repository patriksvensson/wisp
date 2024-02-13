namespace Wisp.Testing;

internal static class TokenExtensions
{
    public static void ShouldBeComment(this PdfObjectToken? token)
    {
        token.ShouldNotBeNull();
        token.Kind.ShouldBe(PdfObjectTokenKind.Comment);
    }

    public static void ShouldBeName(this PdfObjectToken? token, string comment)
    {
        token.ShouldNotBeNull();
        token.Kind.ShouldBe(PdfObjectTokenKind.Name);
        token.Text.ShouldBe(comment);
    }

    public static void ShouldBeStringLiteral(this PdfObjectToken? token, string comment)
    {
        token.ShouldNotBeNull();
        token.Kind.ShouldBe(PdfObjectTokenKind.StringLiteral);
        token.Lexeme.ShouldNotBeNull();
        token.Lexeme.ShouldBe(Encoding.UTF8.GetBytes(comment));
    }

    public static void ShouldBeHexStringLiteral(this PdfObjectToken? token, string comment)
    {
        token.ShouldNotBeNull();
        token.Kind.ShouldBe(PdfObjectTokenKind.HexStringLiteral);
        token.Text.ShouldBe(comment);
    }

    public static void ShouldBeInteger(this PdfObjectToken? token, int number)
    {
        token.ShouldNotBeNull();
        token.Kind.ShouldBe(PdfObjectTokenKind.Integer);
        token.Text.ShouldBe(number.ToString(CultureInfo.InvariantCulture));
    }

    public static void ShouldBeReal(this PdfObjectToken? token, double number)
    {
        token.ShouldNotBeNull();
        token.Kind.ShouldBe(PdfObjectTokenKind.Real);
        token.Text.ShouldBe(number.ToString(CultureInfo.InvariantCulture));
    }
}