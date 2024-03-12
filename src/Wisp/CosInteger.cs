namespace Wisp.Cos;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosInteger : ICosPrimitive
{
    public long Value { get; }
    public int IntValue => (int)Value;

    public CosInteger(long value)
    {
        Value = value;
    }

    public CosInteger(long? value)
    {
        Value = value ?? 0;
    }

    [DebuggerStepThrough]
    public void Accept<TContext>(ICosVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitInteger(this, context);
    }

    public override string ToString()
    {
        return $"[Integer] {Value}";
    }
}