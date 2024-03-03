namespace Wisp.Internal;

internal sealed class NopFilter : Filter
{
    public override byte[] Decode(byte[] data)
    {
        return data;
    }
}