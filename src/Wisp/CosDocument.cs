namespace Wisp.Cos;

[PublicAPI]
public sealed class CosDocument
{
    public PdfVersion Version { get; }
    public CosObjectCollection Objects { get; }
    public CosXRefTable XRefTable { get; }
    public CosTrailer Trailer { get; }
    public CosInfo Info { get; }

    internal CosDocument(
        PdfVersion version,
        CosXRefTable xRefTable,
        CosTrailer trailer,
        CosInfo? info,
        CosObjectResolver? objectResolver)
    {
        ArgumentNullException.ThrowIfNull(xRefTable);

        Objects = new CosObjectCollection(xRefTable, objectResolver);
        Version = version;
        XRefTable = xRefTable;
        Trailer = trailer ?? throw new ArgumentNullException(nameof(trailer));
        Info = info ?? new CosInfo();
    }

    public static CosDocument Open(Stream stream)
    {
        return CosDocumentReader.Read(stream);
    }
}