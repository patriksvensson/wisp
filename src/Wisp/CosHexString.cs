namespace Wisp.Cos;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosHexString : ICosPrimitive
{
    public byte[] Value { get; }

    public CosHexString(byte[] value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    [DebuggerStepThrough]
    public void Accept<TContext>(ICosVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitHexString(this, context);
    }

    public override string ToString()
    {
        var hex = Convert.ToHexString(Value);
        return $"[Hex] {hex}";
    }
}