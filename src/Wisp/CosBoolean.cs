namespace Wisp;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosBoolean : ICosPrimitive
{
    public bool Value { get; }

    public CosBoolean(bool value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return $"[Boolean] {Value}";
    }

    [DebuggerStepThrough]
    public void Accept<TContext>(ICosVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitBoolean(this, context);
    }

    [DebuggerStepThrough]
    public TResult Accept<TContext, TResult>(ICosVisitor<TContext, TResult> visitor, TContext context)
    {
        return visitor.VisitBoolean(this, context);
    }
}