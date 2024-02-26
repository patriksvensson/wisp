namespace Wisp.Cos;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosNull : CosPrimitive
{
    public override string ToString()
    {
        return "[Null]";
    }
}