namespace Wisp.Cos;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosInteger : CosPrimitive
{
    public int Value { get; }

    public CosInteger(int value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return $"[Integer] {Value}";
    }
}