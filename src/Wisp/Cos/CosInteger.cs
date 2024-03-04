namespace Wisp.Cos;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosInteger : CosPrimitive
{
    public long Value { get; }
    public int IntValue => (int)Value;

    public CosInteger(long value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return $"[Integer] {Value}";
    }
}