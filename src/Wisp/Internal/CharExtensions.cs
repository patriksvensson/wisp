namespace Wisp.Internal;

internal static class CharExtensions
{
    public static bool IsPdfWhitespace(this char character)
    {
        switch (character)
        {
            case '\0': // 0x00
            case '\t': // 0x09
            case '\n': // 0x0A
            case '\f': // 0x0C
            case '\r': // 0x0D
            case ' ': // 0x20
                return true;
            default:
                return false;
        }
    }

    public static bool IsPdfName(this char character)
    {
        // Between 0x21 ('!') and 0x7E ('~')?
        var isWithinRange = character >= 0x21 && character <= 0x7E;
        if (!isWithinRange)
        {
            return false;
        }

        // Not part of spec, but things won't work otherwise
        return character is not ('<' or '>' or '/' or '[' or ']' or '(' or ')');
    }

    public static bool IsPdfLineBreak(this char character)
    {
        switch (character)
        {
            case '\n': // 0x0A
            case '\r': // 0x0D
                return true;
            default:
                return false;
        }
    }

    public static bool IsPdfSolidus(this char character)
    {
        // Solidus is the '/' character.
        return character == 0x2F;
    }
}