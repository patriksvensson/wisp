namespace Wisp.Cos;

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

    public override string ToString()
    {
        return $"[Real] {Value.ToString(CultureInfo.InvariantCulture)}";
    }
}