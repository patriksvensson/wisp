namespace Wisp.Filters;

[PublicAPI]
public sealed class RunLengthFilter : Filter
{
    public override string Name { get; } = "RunLengthDecode";
    public override bool Supported { get; } = false;

    public override byte[] Decode(byte[] data, CosDictionary? parameters)
    {
        throw new NotSupportedException();
    }
}