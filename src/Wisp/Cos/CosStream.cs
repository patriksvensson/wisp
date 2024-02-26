namespace Wisp.Cos;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosStream : CosPrimitive
{
    private byte[] _data;

    public CosDictionary Metadata { get; }

    public int Length => Metadata.ReadRequiredInteger(CosName.Known.Length);
    public CosDictionary? DecodeParams => Metadata.GetOptionalValue<CosDictionary>(CosName.Known.DecodeParms);

    public CosStream(CosDictionary metadata, byte[] data)
    {
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        _data = data ?? throw new ArgumentNullException(nameof(data));
    }

    public byte[] GetData()
    {
        // TODO: Decode the data
        return _data;
    }

    public void SetData(byte[]? data, CosDictionary? decodeParameters)
    {
        _data = data ?? Array.Empty<byte>();
        Metadata[CosName.Known.DecodeParms] = decodeParameters;
        Metadata[CosName.Known.Length] = new CosInteger(data?.Length ?? 0);
    }

    public override string ToString()
    {
        return $"[Stream] Length = {Length}";
    }
}