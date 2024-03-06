namespace Wisp.Cos;

[PublicAPI]
public sealed class CosDocument
{
    public PdfVersion Version { get; }
    public CosObjectCollection Objects { get; }
    public CosXRefTable XRefTable { get; }
    public CosTrailer Trailer { get; }

    internal CosDocument(
        PdfVersion version,
        CosXRefTable xRefTable,
        CosTrailer trailer,
        CosObjectResolver? objectResolver)
    {
        ArgumentNullException.ThrowIfNull(xRefTable);

        Objects = new CosObjectCollection(xRefTable, objectResolver);
        Version = version;
        XRefTable = xRefTable;
        Trailer = trailer ?? throw new ArgumentNullException(nameof(trailer));
    }

    public static CosDocument Open(Stream stream)
    {
        return CosDocumentReader.Read(stream);
    }
}