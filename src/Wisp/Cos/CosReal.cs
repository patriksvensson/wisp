namespace Wisp.Cos;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosReal : CosPrimitive
{
    public double Value { get; }

    public CosReal(double value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return $"[Real] {Value}";
    }
}