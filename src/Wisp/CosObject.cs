namespace Wisp;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosObject : ICosPrimitive
{
    public CosObjectId Id { get; }
    public ICosPrimitive Object { get; }

    public CosObject(CosObjectId id, ICosPrimitive obj)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Object = obj ?? throw new ArgumentNullException(nameof(obj));
    }

    [DebuggerStepThrough]
    public void Accept<TContext>(ICosVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitObject(this, context);
    }

    [DebuggerStepThrough]
    public TResult Accept<TContext, TResult>(ICosVisitor<TContext, TResult> visitor, TContext context)
    {
        return visitor.VisitObject(this, context);
    }

    public override string ToString()
    {
        var kind = Object.GetType()?.Name ?? "Unknown";
        return $"[Object] {Id.Number}:{Id.Generation} ({kind})";
    }
}