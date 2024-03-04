namespace Wisp.Filters;

[PublicAPI]
public sealed class AsciiHexFilter : Filter
{
    public override string Name { get; } = "ASCIIHexDecode";
    public override bool Supported { get; } = false;

    public override byte[] Decode(byte[] data, CosDictionary? parameters)
    {
        throw new NotSupportedException();
    }
}