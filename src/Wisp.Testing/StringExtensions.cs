namespace Wisp.Testing;

public static class StringExtensions
{
    public static Stream ToStream(this string text)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(text);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}