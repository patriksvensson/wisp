namespace Wisp;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosStream : ICosPrimitive
{
    private byte[] _data;

    public CosDictionary Dictionary { get; }
    public long Length => Dictionary.GetInt64(CosNames.Length) ?? throw new InvalidOperationException("/Length missing from stream");
    public CosDictionary? DecodeParms => Dictionary.GetDictionary(CosNames.DecodeParms);

    public bool IsCompressed => Dictionary.ContainsKey(CosNames.Filter);

    public CosStream(CosDictionary dictionary, byte[] data)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));

        Dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        if (!Dictionary.ContainsKey(CosNames.Length))
        {
            Dictionary.Add(CosNames.Length, new CosInteger(_data.Length));
        }
    }

    public byte[] GetData()
    {
        return _data;
    }

    public byte[] GetUnfilteredData()
    {
        if (Dictionary.ContainsKey(CosNames.Filter))
        {
            return Filter.Decode(this, _data);
        }

        return _data;
    }

    public void Compress(CosCompression compression)
    {
        if (!Dictionary.ContainsKey(CosNames.Filter))
        {
            _data = FlateFilter.Encode(_data, compression);

            Dictionary.Set(CosNames.Filter, new CosName("FlateDecode"));
            Dictionary.Set(CosNames.DecodeParms, null);
            Dictionary.Set(CosNames.Length, new CosInteger(_data.Length));
        }
    }

    public void Decompress()
    {
        if (Dictionary.ContainsKey(CosNames.Filter))
        {
            _data = Filter.Decode(this, _data);

            Dictionary.Set(CosNames.Filter, null);
            Dictionary.Set(CosNames.DecodeParms, null);
            Dictionary.Set(CosNames.Length, new CosInteger(_data.Length));
        }
    }

    public void SetData(byte[]? data, CosDictionary? decodeParameters)
    {
        _data = data ?? [];
        Dictionary.Set(CosNames.Filter, null);
        Dictionary.Set(CosNames.DecodeParms, decodeParameters);
        Dictionary.Set(CosNames.Length, new CosInteger(data?.Length ?? 0));
    }

    [DebuggerStepThrough]
    public void Accept<TContext>(ICosVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitStream(this, context);
    }

    [DebuggerStepThrough]
    public TResult Accept<TContext, TResult>(ICosVisitor<TContext, TResult> visitor, TContext context)
    {
        return visitor.VisitStream(this, context);
    }

    public override string ToString()
    {
        return $"[Stream] Length = {Length}";
    }
}