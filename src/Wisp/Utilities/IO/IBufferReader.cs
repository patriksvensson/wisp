namespace Wisp;

public interface IBufferReader
{
    bool CanRead { get; }
    int Position { get; }

    int Seek(long offset, SeekOrigin origin);

    int PeekByte();
    int ReadByte();

    ReadOnlySpan<byte> ReadBytes(int count);
}