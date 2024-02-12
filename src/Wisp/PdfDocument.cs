namespace Wisp;

public sealed class PdfDocument : IDisposable, IPdfReaderContext
{
    private readonly PdfReader _reader;

    public PdfHeader Header { get; }
    public PdfXRefTable XRefTable { get; }
    public PdfTrailer Trailer { get; }

    PdfObjectCache IPdfReaderContext.Cache { get; } = new();

    internal PdfDocument(
        PdfHeader header,
        PdfXRefTable xRefTable,
        PdfTrailer trailer,
        PdfReader reader)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));

        Header = header ?? throw new ArgumentNullException(nameof(header));
        XRefTable = xRefTable ?? throw new ArgumentNullException(nameof(xRefTable));
        Trailer = trailer ?? throw new ArgumentNullException(nameof(trailer));
    }

    public static PdfDocument Read(Stream stream)
    {
        return PdfDocumentReader.Read(stream);
    }

    public bool TryReadObject(PdfObjectId id, [NotNullWhen(true)] out PdfObject? result)
    {
        return _reader.TryReadObject(this, id, out result);
    }

    public bool TryReadObject(PdfXRef xref, [NotNullWhen(true)] out PdfObject? result)
    {
        return _reader.TryReadObject(this, xref, out result);
    }

    public void Dispose()
    {
        _reader.Dispose();
    }
}