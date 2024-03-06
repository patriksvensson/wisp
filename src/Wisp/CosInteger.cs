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

    public override string ToString()
    {
        return $"[Integer] {Value}";
    }
}