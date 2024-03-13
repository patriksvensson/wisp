namespace Wisp.Internal;

internal sealed class ByteEncoding : Encoding
{
    public static ByteEncoding Shared { get; } = new();

    public override int GetByteCount(char[] chars, int index, int count)
    {
        return count;
    }

    public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
    {
        for (var count = charCount; count > 0; charIndex++, byteIndex++, count--)
        {
            bytes[byteIndex] = (byte)chars[charIndex];
        }

        return charCount;
    }

    public override int GetCharCount(byte[] bytes, int index, int count)
    {
        return count;
    }

    public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
    {
        for (var count = byteCount; count > 0; charIndex++, byteIndex++, count--)
        {
            chars[charIndex] = (char)bytes[byteIndex];
        }

        return byteCount;
    }

    public override int GetMaxByteCount(int charCount)
    {
        return charCount;
    }

    public override int GetMaxCharCount(int byteCount)
    {
        return byteCount;
    }
}