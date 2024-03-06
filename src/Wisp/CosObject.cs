namespace Wisp.Cos;

[PublicAPI]
public sealed class CosObject : ICosPrimitive
{
    public CosObjectId Id { get; }
    public ICosPrimitive Object { get; }

    public CosObject(CosObjectId id, ICosPrimitive obj)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Object = obj ?? throw new ArgumentNullException(nameof(obj));
    }
}