namespace Wisp.Internal;

internal static class ByteStreamReaderExtensions
{
    public static char PeekChar(this IByteStreamReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        return (char)reader.PeekByte();
    }

    public static char ReadChar(this IByteStreamReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        return (char)reader.ReadByte();
    }

    public static void Discard(this IByteStreamReader reader)
    {
        reader.ReadByte();
    }

    public static void Discard(this IByteStreamReader reader, char expected)
    {
        var read = ReadChar(reader);
        if (read != expected)
        {
            throw new InvalidOperationException($"Expected '{expected}' but got '{read}'.");
        }
    }
}