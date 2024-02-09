using System.Globalization;
using System.Text;

namespace Wisp;

internal static class HexUtility
{
    public static string FromHex(string hex)
    {
        if (hex.Length % 2 != 0)
        {
            hex += "0";
        }

        var builder = new StringBuilder();
        for (var i = 0; i < hex.Length; i += 2)
        {
            builder.Append(FromHex(hex[i], hex[i + 1]));
        }

        return builder.ToString();
    }

    public static char FromHex(char first, char second)
    {
        // https://stackoverflow.com/a/7874155/936
        return (char)(short)int.Parse(
            new string(new[] { first, second }),
            NumberStyles.HexNumber);
    }
}