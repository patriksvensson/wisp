namespace Wisp.Cos;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosStream : CosPrimitive
{
    private byte[] _data;
    private bool _decoded;

    public CosDictionary Metadata { get; }

    public long Length => Metadata.GetInt64(CosNames.Length)
                          ?? throw new InvalidOperationException("/Length missing from stream");

    public CosDictionary? DecodeParms => Metadata.GetDictionary(CosNames.DecodeParms);

    public CosStream(CosDictionary metadata, byte[] data)
    {
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        _data = data ?? throw new ArgumentNullException(nameof(data));
    }

    public byte[] GetData()
    {
        if (!_decoded)
        {
            _data = Filter.Decode(this, _data);
            _decoded = true;
        }

        return _data;
    }

    public void SetData(byte[]? data, CosDictionary? decodeParameters)
    {
        _data = data ?? [];
        Metadata[CosNames.DecodeParms] = decodeParameters;
        Metadata[CosNames.Length] = new CosInteger(data?.Length ?? 0);
    }

    public override string ToString()
    {
        return $"[Stream] Length = {Length}";
    }
}