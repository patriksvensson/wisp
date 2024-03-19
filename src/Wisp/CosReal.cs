namespace Wisp;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosReal : ICosPrimitive
{
    public double Value { get; }

    public CosReal(double value)
    {
        Value = value;
    }

    [DebuggerStepThrough]
    public void Accept<TContext>(ICosVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitReal(this, context);
    }

    [DebuggerStepThrough]
    public TResult Accept<TContext, TResult>(ICosVisitor<TContext, TResult> visitor, TContext context)
    {
        return visitor.VisitReal(this, context);
    }

    public override string ToString()
    {
        return $"[Real] {Value.ToString(CultureInfo.InvariantCulture)}";
    }
}