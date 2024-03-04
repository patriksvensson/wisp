namespace Wisp.Filters;

[PublicAPI]
public sealed class Ascii85Filter : Filter
{
    public override string Name { get; } = "ASCII85Decode";
    public override bool Supported { get; } = false;

    public override byte[] Decode(byte[] data, CosDictionary? parameters)
    {
        throw new NotSupportedException();
    }
}