namespace Wisp.Cos;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosObjectStream : CosPrimitive
{
    private readonly CosStream _stream;
    private readonly List<(int Id, long Offset)> _offsetsByIndex = new();
    private readonly Dictionary<int, long> _offsetsById = new();
    private bool _unpacked;

    /// <summary>
    /// Gets the number of indirect objects stored in the stream.
    /// </summary>
    public int N => _stream.Metadata.GetInt32(CosNames.N) ?? 0;

    public CosObjectStream(CosStream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
    }

    public CosObject GetObject(CosObjectId id)
    {
        var bytes = _stream.GetData();
        if (bytes == null)
        {
            throw new InvalidOperationException("Stream contained no data");
        }

        if (_unpacked)
        {
            if (!_offsetsById.ContainsKey(id.Number))
            {
                throw new InvalidOperationException("Object does not exist within object stream");
            }
        }

        using (var stream = new MemoryStream(bytes))
        {
            var parser = new CosParser(stream, true);
            EnsureOffsetsHaveBeenPopulated(parser);

            // Ensure the object exist within the stream
            if (!_offsetsById.TryGetValue(id.Number, out var offset))
            {
                throw new InvalidOperationException("Object does not exist within object stream");
            }

            // Read the object at the correct offset
            parser.Lexer.Reader.Seek(offset, SeekOrigin.Begin);
            var obj = parser.ParseObject();

            return new CosObject(id, obj);
        }
    }

    public CosObject GetObjectByIndex(int index)
    {
        var bytes = _stream.GetData();
        if (bytes == null)
        {
            throw new InvalidOperationException("Stream contained no data");
        }

        using (var stream = new MemoryStream(bytes))
        {
            var parser = new CosParser(stream, true);
            EnsureOffsetsHaveBeenPopulated(parser);

            // Read the object at the correct offset
            var (id, offset) = _offsetsByIndex[index];
            parser.Lexer.Reader.Seek(offset, SeekOrigin.Begin);
            var obj = parser.ParseObject();

            return new CosObject(
                new CosObjectId(id, 0),
                obj);
        }
    }

    private void EnsureOffsetsHaveBeenPopulated(CosParser parser)
    {
        if (_unpacked)
        {
            return;
        }

        var objectOffset = _stream.Metadata.GetInt64(CosNames.First);
        if (objectOffset == null)
        {
            throw new InvalidOperationException("Object stream is missing /First parameter");
        }

        for (var i = 0; i < N; i++)
        {
            if (!parser.CanRead)
            {
                throw new InvalidOperationException("Encountered premature end of object stream");
            }

            var id = (int)((CosInteger)parser.ParseObject()).Value;
            var offset = objectOffset.Value + ((CosInteger)parser.ParseObject()).Value;

            _offsetsById.Add(id, offset);
            _offsetsByIndex.Add((id, offset));
        }

        _unpacked = true;
    }

    public override string ToString()
    {
        return $"[ObjectStream] Objects = {N}, Size = {_stream.Length}";
    }
}