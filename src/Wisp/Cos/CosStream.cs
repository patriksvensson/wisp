namespace Wisp.Cos;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosStream : CosPrimitive
{
    private byte[] _data;
    private bool _decoded;

    public CosDictionary Metadata { get; }

    public long Length => Metadata.GetRequiredInteger(CosName.Known.Length);
    public CosDictionary? DecodeParams => Metadata.GetOptional<CosDictionary>(CosName.Known.DecodeParms);

    public CosStream(CosDictionary metadata, byte[] data)
    {
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        _data = data ?? throw new ArgumentNullException(nameof(data));
    }

    public byte[] GetData()
    {
        if (!_decoded)
        {
            var pipeline = FilterPipeline.Create(Metadata);

            _data = pipeline.Decode(_data, Metadata);
            _decoded = true;
        }

        return _data;
    }

    public void SetData(byte[]? data, CosDictionary? decodeParameters)
    {
        _data = data ?? [];
        Metadata[CosName.Known.DecodeParms] = decodeParameters;
        Metadata[CosName.Known.Length] = new CosInteger(data?.Length ?? 0);
    }

    public override string ToString()
    {
        return $"[Stream] Length = {Length}";
    }
}