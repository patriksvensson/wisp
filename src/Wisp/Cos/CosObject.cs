namespace Wisp.Cos;

[PublicAPI]
public sealed class CosObject : CosPrimitive
{
    public CosObjectId Id { get; }
    public CosPrimitive Object { get; }

    public CosObject(CosObjectId id, CosPrimitive obj)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Object = obj ?? throw new ArgumentNullException(nameof(obj));
    }
}