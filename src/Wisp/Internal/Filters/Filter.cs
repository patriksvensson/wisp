namespace Wisp.Internal;

internal abstract class Filter
{
    public abstract byte[] Decode(byte[] data);
}