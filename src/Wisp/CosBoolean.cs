namespace Wisp.Cos;

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
}