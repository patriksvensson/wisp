namespace Wisp.Filters;

[PublicAPI]
public sealed class DctFilter : Filter
{
    public override string Name { get; } = "DCTDecode";
    public override bool Supported { get; } = false;

    public override byte[] Decode(byte[] data, CosDictionary? parameters)
    {
        throw new NotSupportedException();
    }
}