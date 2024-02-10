namespace Wisp;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PdfStream : PdfObject
{
    private readonly PdfDictionary _metadata;
    private byte[] _data;

    public PdfDictionary Metadata => _metadata;

    public int Length => _metadata.ReadRequiredInteger(PdfName.Known.Length);
    public PdfDictionary? DecodeParams => _metadata.GetOptionalValue<PdfDictionary>(PdfName.Known.DecodeParms);

    public PdfStream(PdfDictionary metadata, byte[] data)
    {
        _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        _data = data ?? throw new ArgumentNullException(nameof(data));
    }

    public byte[] GetData()
    {
        // TODO: Decode the data
        return _data;
    }

    public void SetData(byte[]? data, PdfDictionary? decodeParameters)
    {
        _data = data ?? Array.Empty<byte>();
        _metadata[PdfName.Known.DecodeParms] = decodeParameters;
        _metadata[PdfName.Known.Length] = new PdfInteger(data?.Length ?? 0);
    }

    public override string ToString()
    {
        return $"[Stream] Length = {Length}";
    }
}