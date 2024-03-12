namespace Wisp.Cos;

[PublicAPI]
public sealed class CosDocument
{
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
        CosObjectCache objects)
    {
        ArgumentNullException.ThrowIfNull(xRefTable);
        ArgumentNullException.ThrowIfNull(objects);

        Objects = new CosObjectCacheFrontend(objects);
        Version = version;
        XRefTable = xRefTable;
        Trailer = trailer ?? throw new ArgumentNullException(nameof(trailer));

        _info = info;
    }

    public static CosDocument Open(Stream stream)
    {
        return CosDocumentReader.Read(stream);
    }

    public void Save(Stream stream, CosCompression compression = CosCompression.Optimal)
    {
        using var writer = new CosWriter(
            this, stream,
            new CosWriterSettings
            {
                Compression = compression,
            });

        CosDocumentWriter.Write(writer);
    }

    private CosInfo CreateInfo()
    {
        var obj = new CosObject(XRefTable.GetNextId(), new CosDictionary());
        Objects.Set(obj);
        Trailer.Info = new CosObjectReference(obj.Id);
        return new CosInfo(obj);
    }
}