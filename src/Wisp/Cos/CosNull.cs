namespace Wisp.Cos;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosNull : CosPrimitive
{
    public static CosNull Shared { get; } = new();

    public override string ToString()
    {
        return "[Null]";
    }
}