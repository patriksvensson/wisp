namespace Wisp.Cos;

[PublicAPI]
public sealed class CosDocument
{
    public PdfVersion Version { get; }
    public CosObjectCollection Objects { get; }
    public CosXRefTable XRefTable { get; }
    public CosDictionary Trailer { get; }

    public CosDocument(
        PdfVersion version, CosXRefTable xRefTable,
        CosDictionary trailer, CosObjectResolver? resolver)
    {
        ArgumentNullException.ThrowIfNull(xRefTable);

        Objects = new CosObjectCollection(xRefTable, resolver);
        Version = version;
        XRefTable = xRefTable;
        Trailer = trailer ?? throw new ArgumentNullException(nameof(trailer));
    }

    public static CosDocument Open(Stream stream)
    {
        return CosDocumentReader.Read(stream);
    }
}