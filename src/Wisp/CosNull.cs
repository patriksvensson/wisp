namespace Wisp.Cos;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosNull : ICosPrimitive
{
    public static CosNull Shared { get; } = new();

    [DebuggerStepThrough]
    public void Accept<TContext>(ICosVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitNull(this, context);
    }

    public override string ToString()
    {
        return "[Null]";
    }
}