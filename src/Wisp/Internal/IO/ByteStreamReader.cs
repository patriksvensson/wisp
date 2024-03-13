namespace Wisp.Internal;

internal sealed class ByteStreamReader : IByteStreamReader
{
    private readonly ReadOnlyMemory<byte> _buffer;
    private int _position;

    public bool CanRead => _position < _buffer.Length;
    public long Position => _position;
    public long Length => _buffer.Length;

    public ByteStreamReader(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        _buffer = new ReadOnlyMemory<byte>(ReadAllBytes(stream));
        _position = 0;
    }

    public void Dispose()
    {
    }

    public int PeekByte()
    {
        if (_position >= _buffer.Length)
        {
            return -1;
        }

        return _buffer.Span[_position];
    }

    public int ReadByte()
    {
        var result = PeekByte();
        if (result != -1)
        {
            _position++;
        }

        return result;
    }

    public ReadOnlySpan<byte> ReadBytes(int count)
    {
        if (_position + count > _buffer.Length)
        {
            throw new InvalidOperationException("Exceeded stream end");
        }

        var result = _buffer.Slice(_position, count).Span;
        _position += count;

        return result;
    }

    public long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                _position = (int)offset;
                break;
            case SeekOrigin.Current:
                _position += (int)offset;
                break;
            case SeekOrigin.End:
                _position = _buffer.Length + (int)offset;
                break;
            default:
                throw new NotSupportedException("Unknown seek origin");
        }

        return _position;
    }

    private static byte[] ReadAllBytes(Stream stream)
    {
        if (stream is MemoryStream memoryStream)
        {
            return memoryStream.ToArray();
        }

        // TODO: Read using buffer
        using var output = new MemoryStream();
        stream.CopyTo(output);
        return output.ToArray();
    }
}