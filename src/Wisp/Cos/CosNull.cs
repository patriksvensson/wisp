namespace Wisp.Cos;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosNull : CosPrimitive
{
    public override string ToString()
    {
        return "[Null]";
    }
}