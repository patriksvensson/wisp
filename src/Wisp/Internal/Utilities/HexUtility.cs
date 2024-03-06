namespace Wisp.Internal;

internal static class HexUtility
{
    public static char FromHex(char first, char second)
    {
        // TODO: Optimize this a bit :)
        // https://stackoverflow.com/a/7874155/936
        return (char)(short)int.Parse(
            new string(new[] { first, second }),
            NumberStyles.HexNumber);
    }
}