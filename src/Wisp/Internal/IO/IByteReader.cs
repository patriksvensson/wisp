namespace Wisp.Internal;

internal interface IByteReader : IDisposable
{
    bool CanRead { get; }
    long Position { get; }
    long Length { get; }

    long Seek(long offset, SeekOrigin origin);

    int PeekByte();
    int ReadByte();

    ReadOnlySpan<byte> ReadBytes(int count);
}