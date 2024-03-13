namespace Wisp.Internal;

internal static class StreamExtensions
{
    public static Stream Append(this Stream destination, Stream source)
    {
        destination.Position = destination.Length;
        source.Position = 0;
        source.CopyTo(destination);

        return destination;
    }
}