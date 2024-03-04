namespace Wisp.Filters;

[PublicAPI]
public sealed class LzwFilter : Filter
{
    public override string Name { get; } = "LZWDecode";
    public override bool Supported { get; } = false;

    public override byte[] Decode(byte[] data, CosDictionary parameters)
    {
        throw new NotSupportedException();
    }
}