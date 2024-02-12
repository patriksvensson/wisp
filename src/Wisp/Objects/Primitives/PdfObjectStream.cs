namespace Wisp;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PdfObjectStream : PdfObject
{
    private readonly PdfStream _stream;
    private readonly List<int> _offsets = new List<int>();
    private bool _unpacked;

    public int ObjectCount => _stream.Metadata.ReadOptionalInteger(PdfName.Known.N) ?? 0;
    public int Length => _stream.Length;

    public PdfObjectStream(PdfStream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
    }

    public PdfObject GetObjectByIndex(int index)
    {
        var bytes = _stream.GetData();
        if (bytes == null)
        {
            throw new InvalidOperationException("Stream contained no data");
        }

        var stream = new MemoryStream(bytes);
        var parser = new PdfObjectParser(new ByteReader(stream), true);

        var offsets = GetOffsets(parser);

        // Go to the object position.
        var offset = offsets[index];
        parser.Lexer.Reader.Seek(offset, SeekOrigin.Begin);

        return parser.ParseObject();
    }

    private List<int> GetOffsets(PdfObjectParser parser)
    {
        if (_unpacked)
        {
            return _offsets;
        }

        var objectCount = _stream.Metadata.ReadRequiredInteger(PdfName.Known.N);
        var objectOffset = _stream.Metadata.ReadRequiredInteger(PdfName.Known.First);

        var numbers = new List<int>();
        for (var i = 0; i < objectCount; i++)
        {
            if (!parser.Lexer.Peek(out var token))
            {
                throw new InvalidOperationException("Encountered premature end of object stream");
            }

            numbers.Add(((PdfInteger)parser.ParseObject()).Value);
            _offsets.Add(objectOffset + ((PdfInteger)parser.ParseObject()).Value);
        }

        _unpacked = true;

        return _offsets;
    }

    public override void Accept<TContext>(PdfObjectVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitObjectStream(this, context);
    }

    public override string ToString()
    {
        return $"[ObjectStream] Objects = {ObjectCount}, Size = {_stream.Length}";
    }
}