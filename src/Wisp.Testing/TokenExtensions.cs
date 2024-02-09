using System.Globalization;
using System.Text;
using Shouldly;

namespace Wisp.Testing;

public static class TokenExtensions
{
    public static void ShouldBeComment(this Token? token, string comment)
    {
        token.ShouldNotBeNull();
        token.Kind.ShouldBe(TokenKind.Comment);
        token.Text.ShouldBe(comment);
    }

    public static void ShouldBeName(this Token? token, string comment)
    {
        token.ShouldNotBeNull();
        token.Kind.ShouldBe(TokenKind.Name);
        token.Text.ShouldBe(comment);
    }

    public static void ShouldBeStringLiteral(this Token? token, string comment)
    {
        token.ShouldNotBeNull();
        token.Kind.ShouldBe(TokenKind.StringLiteral);
        token.Lexeme.ShouldNotBeNull();
        token.Lexeme.ShouldBe(Encoding.UTF8.GetBytes(comment));
    }

    public static void ShouldBeHexStringLiteral(this Token? token, string comment)
    {
        token.ShouldNotBeNull();
        token.Kind.ShouldBe(TokenKind.HexStringLiteral);
        token.Text.ShouldBe(comment);
    }

    public static void ShouldBeInteger(this Token? token, int number)
    {
        token.ShouldNotBeNull();
        token.Kind.ShouldBe(TokenKind.Integer);
        token.Text.ShouldBe(number.ToString(CultureInfo.InvariantCulture));
    }

    public static void ShouldBeReal(this Token? token, double number)
    {
        token.ShouldNotBeNull();
        token.Kind.ShouldBe(TokenKind.Real);
        token.Text.ShouldBe(number.ToString(CultureInfo.InvariantCulture));
    }
}