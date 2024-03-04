namespace Wisp.Filters;

public abstract class Filter
{
    public abstract byte[] Decode(byte[] data);
}