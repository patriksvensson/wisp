namespace Wisp.Cos;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosBoolean : CosPrimitive
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
}