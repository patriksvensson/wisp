namespace Wisp.Filters;

[PublicAPI]
public sealed class Jbig2Filter : Filter
{
    public override string Name { get; } = "JBIG2Decode";
    public override bool Supported { get; } = false;

    public override byte[] Decode(byte[] data, CosDictionary? parameters)
    {
        throw new NotSupportedException();
    }
}