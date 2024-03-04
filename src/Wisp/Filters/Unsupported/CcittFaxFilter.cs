namespace Wisp.Filters;

[PublicAPI]
public sealed class CcittFaxFilter : Filter
{
    public override string Name { get; } = "CCITTFaxDecode";
    public override bool Supported { get; } = false;

    public override byte[] Decode(byte[] data, CosDictionary parameters)
    {
        throw new NotSupportedException();
    }
}