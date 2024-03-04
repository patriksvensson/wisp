namespace Wisp.Filters;

[PublicAPI]
public abstract class Filter
{
    public abstract string Name { get; }
    public virtual bool Supported { get; } = true;
    public abstract byte[] Decode(byte[] data, CosDictionary parameters);
}