namespace Wisp;

internal static class BufferReaderExtensions
{
    public static char PeekChar(this IBufferReader reader)
    {
        if (reader is null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        return (char)reader.PeekByte();
    }

    public static char ReadChar(this IBufferReader reader)
    {
        if (reader is null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        return (char)reader.ReadByte();
    }

    public static void Discard(this IBufferReader reader)
    {
        reader.ReadByte();
    }

    public static void Discard(this IBufferReader reader, char expected)
    {
        var read = ReadChar(reader);
        if (read != expected)
        {
            throw new InvalidOperationException($"Expected '{expected}' but got '{read}'.");
        }
    }
}