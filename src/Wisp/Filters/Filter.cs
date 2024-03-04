namespace Wisp.Filters;

[PublicAPI]
public abstract class Filter
{
    public abstract string Name { get; }
    public virtual bool Supported { get; } = true;

    public abstract byte[] Decode(byte[] data, CosDictionary? parameters);

    public static byte[] Decode(CosStream stream, byte[] data)
    {
        var pipeline = FilterPipeline.Factory.Create(stream);
        return pipeline.Decode(data, stream.DecodeParms);
    }
}