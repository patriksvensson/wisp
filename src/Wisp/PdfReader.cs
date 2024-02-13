namespace Wisp;

internal sealed class PdfReader : IDisposable
{
    private readonly PdfObjectParser _parser;

    public int Position => _parser.Lexer.Reader.Position;
    public int Length => _parser.Lexer.Reader.Length;
    public bool CanRead => _parser.Lexer.Reader.CanRead;

    public PdfReader(Stream stream)
    {
        _parser = new PdfObjectParser(stream);
    }

    public void Dispose()
    {
        _parser.Dispose();
    }

    public int Seek(int offset, SeekOrigin origin)
    {
        return _parser.Lexer.Reader.Seek(offset, origin);
    }

    public int ReadByte()
    {
        return _parser.Lexer.Reader.ReadByte();
    }

    public ReadOnlySpan<byte> ReadBytes(int count)
    {
        return _parser.Lexer.Reader.ReadBytes(count);
    }

    public PdfObjectToken? PeekToken()
    {
        _parser.Lexer.Peek(out var token);
        return token;
    }

    public PdfObjectToken ReadToken()
    {
        return _parser.Lexer.Read();
    }

    public PdfObject ReadObject()
    {
        return _parser.ParseObject();
    }

    public bool TryReadObject(IPdfReaderContext context, PdfObjectId id, [NotNullWhen(true)] out PdfObject? result)
    {
        // Already know about this object?
        if (context.Cache.TryGetObject(id, out var obj))
        {
            result = obj;
            return true;
        }

        var xref = context.XRefTable.GetRequiredXRef(id);
        if (xref is PdfFreeXRef)
        {
            result = null;
            return false;
        }

        if (TryReadObject(context, xref, out var objFromXRef))
        {
            result = objFromXRef;
            return true;
        }

        result = null;
        return false;
    }

    public bool TryReadObject(IPdfReaderContext context, PdfXRef xref, [NotNullWhen(true)] out PdfObject? obj)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (xref is null)
        {
            throw new ArgumentNullException(nameof(xref));
        }

        var position = _parser.Lexer.Reader.Position;

        try
        {
            obj = ReadObject(context, xref);
            return true;
        }
        catch
        {
            _parser.Lexer.Reader.Seek(position, SeekOrigin.Begin);

            obj = null;
            return false;
        }
    }

    private PdfObject ReadObject(IPdfReaderContext context, PdfXRef xref)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(xref);

        return xref switch
        {
            PdfFreeXRef => throw new InvalidOperationException("Trying to read a free object"),
            PdfStreamXRef stream => ReadObjectStream(context, stream),
            PdfIndirectXRef indirect => ReadIndirectObject(context, indirect),
            _ => throw new NotSupportedException("Unknown xref type"),
        };
    }

    private PdfObject ReadIndirectObject(IPdfReaderContext context, PdfIndirectXRef xref)
    {
        _parser.Lexer.Reader.Seek(xref.Position, SeekOrigin.Begin);
        var obj = _parser.ParseObject();

        if (obj is PdfObjectDefinition objDefinition)
        {
            // Make sure we got what we wanted
            if (objDefinition.Id.Number != xref.Id.Number)
            {
                throw new InvalidOperationException(
                    $"Read object {xref.Id} but got {objDefinition.Id}");
            }

            context.Cache.AddOrUpdate(objDefinition);
        }
        else
        {
            throw new InvalidOperationException($"Expected to find object definition at position {xref.Position}");
        }

        return obj;
    }

    private PdfObject ReadObjectStream(IPdfReaderContext context, PdfStreamXRef xref)
    {
        if (xref.StreamId == null)
        {
            throw new InvalidOperationException("Not a stream xref");
        }

        // Get the stream xref
        var streamXref = context.XRefTable.GetReference(xref.StreamId);
        if (streamXref == null)
        {
            throw new InvalidOperationException("Could not find stream xref");
        }

        // Read the object definition for the stream
        var definition = ReadObject(context, streamXref) as PdfObjectDefinition;
        if (definition == null || definition.Object is not PdfObjectStream stream)
        {
            throw new InvalidOperationException("Could not read object stream or object was of the wrong type");
        }

        // Read the object from the stream
        var obj = stream.GetObjectByIndex(xref.Index);

        // Add to the cache and return the object
        context.Cache.AddOrUpdate(new PdfObjectDefinition(xref.Id, obj));
        return obj;
    }
}