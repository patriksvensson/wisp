namespace Wisp;

public interface IByteReader : IDisposable
{
    bool CanRead { get; }
    int Position { get; }
    int Length { get; }

    int Seek(long offset, SeekOrigin origin);

    int PeekByte();
    int ReadByte();

    ReadOnlySpan<byte> ReadBytes(int count);
}