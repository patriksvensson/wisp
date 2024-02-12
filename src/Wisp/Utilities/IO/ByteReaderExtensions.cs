namespace Wisp;

internal static class ByteReaderExtensions
{
    public static char PeekChar(this IByteReader reader)
    {
        if (reader is null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        return (char)reader.PeekByte();
    }

    public static char ReadChar(this IByteReader reader)
    {
        if (reader is null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        return (char)reader.ReadByte();
    }

    public static void Discard(this IByteReader reader)
    {
        reader.ReadByte();
    }

    public static void Discard(this IByteReader reader, char expected)
    {
        var read = ReadChar(reader);
        if (read != expected)
        {
            throw new InvalidOperationException($"Expected '{expected}' but got '{read}'.");
        }
    }
}