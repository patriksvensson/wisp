using System.Globalization;

namespace Wisp.Extensions;

public static class CharExtensions
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
}