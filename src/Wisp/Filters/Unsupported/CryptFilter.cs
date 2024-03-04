namespace Wisp.Filters;

[PublicAPI]
public sealed class CryptFilter : Filter
{
    public override string Name { get; } = "Crypt";
    public override bool Supported { get; } = false;

    public override byte[] Decode(byte[] data, CosDictionary parameters)
    {
        throw new NotSupportedException();
    }
}