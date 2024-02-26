namespace Wisp.Internal;

internal interface IByteStreamReader : IDisposable
{
    bool CanRead { get; }
    long Position { get; }
    long Length { get; }

    long Seek(long offset, SeekOrigin origin);

    int PeekByte();
    int ReadByte();

    ReadOnlySpan<byte> ReadBytes(int count);
}