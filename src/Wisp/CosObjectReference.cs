namespace Wisp;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public class CosObjectReference : ICosPrimitive, IEquatable<CosObjectReference>
{
    public CosObjectId Id { get; set; }

    public static CosObjectReferenceComparer Comparer => CosObjectReferenceComparer.Shared;

    public CosObjectReference(int number, int generation)
    {
        Id = new CosObjectId(number, generation);
    }

    public CosObjectReference(CosObjectId id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is CosObjectId objectId)
        {
            return Equals(objectId);
        }

        return false;
    }

    public bool Equals(CosObjectReference? other)
    {
        return CosObjectReferenceComparer.Shared.Equals(this, other);
    }

    public override int GetHashCode()
    {
        return CosObjectReferenceComparer.Shared.GetHashCode(this);
    }

    [DebuggerStepThrough]
    public void Accept<TContext>(ICosVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitObjectReference(this, context);
    }

    public override string ToString()
    {
        return $"[ObjectReference] {Id.Number}:{Id.Generation}".Trim();
    }
}

[PublicAPI]
public class CosObjectReference<T> : CosObjectReference
    where T : class, ICosPrimitive
{
    public T Object { get; set; }

    public CosObjectReference(CosObject obj)
        : base(obj.Id)
    {
        ArgumentNullException.ThrowIfNull(obj);

        Object = obj.Object as T ?? throw new InvalidOperationException(
            "Typed object reference was not of the expected type");
    }

    internal CosObjectReference(CosObjectReference id, T obj)
        : base(id.Id.Number, id.Id.Generation)
    {
        Object = obj ?? throw new ArgumentNullException(nameof(obj));
    }
}

[PublicAPI]
public sealed class CosObjectReferenceComparer : IEqualityComparer<CosObjectReference>
{
    public static CosObjectReferenceComparer Shared { get; } = new();

    public bool Equals(CosObjectReference? x, CosObjectReference? y)
    {
        if (x == null && y == null)
        {
            return true;
        }

        if (x == null || y == null)
        {
            return false;
        }

        return CosObjectIdComparer.Shared.Equals(x.Id, y.Id);
    }

    public int GetHashCode(CosObjectReference obj)
    {
        return CosObjectIdComparer.Shared.GetHashCode(obj.Id);
    }
}