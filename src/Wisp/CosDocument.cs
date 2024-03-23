namespace Wisp;

[PublicAPI]
public sealed class CosDocument : IDisposable
{
    private readonly CosObjectResolver? _resolver;
    private CosInfo? _info;

    public PdfVersion Version { get; }
    public ICosObjectCache Objects { get; }
    public CosXRefTable XRefTable { get; }
    public CosTrailer Trailer { get; }
    public CosInfo Info => _info ??= CreateInfo();

    public CosDocument()
    {
        Version = PdfVersion.Pdf1_7;
        XRefTable = new CosXRefTable();
        Objects = new CosObjectCache(XRefTable, null);
        Trailer = new CosTrailer(new CosDictionary());
    }

    internal CosDocument(
        PdfVersion version,
        CosXRefTable xRefTable,
        CosTrailer trailer,
        CosInfo? info,
        CosObjectCache objects,
        CosObjectResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(xRefTable);
        ArgumentNullException.ThrowIfNull(resolver);

        Objects = objects ?? throw new ArgumentNullException(nameof(objects));
        Version = version;
        XRefTable = xRefTable;
        Trailer = trailer ?? throw new ArgumentNullException(nameof(trailer));

        _info = info;
        _resolver = resolver;
    }

    void IDisposable.Dispose()
    {
        Close();
    }

    public void Close()
    {
        _resolver?.Dispose();
    }

    public static CosDocument Open(
        Stream stream,
        CosReaderSettings? settings = null)
    {
        return CosDocumentReader.Read(stream, settings);
    }

    public void Save(
        Stream stream,
        CosWriterSettings? settings = null)
    {
        settings ??= new CosWriterSettings
        {
            Compression = CosCompression.Optimal,
            UnpackObjectStreams = false,
            LeaveStreamOpen = false,
        };

        using var writer = new CosWriter(stream, settings);
        CosDocumentWriter.Write(this, writer);
    }

    private CosInfo CreateInfo()
    {
        var obj = new CosObject(XRefTable.GetNextId(), new CosDictionary());
        Objects.Set(obj);
        Trailer.Info = new CosObjectReference(obj.Id);
        return new CosInfo(obj);
    }
}