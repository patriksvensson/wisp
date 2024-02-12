namespace Wisp;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PdfNull : PdfObject
{
    public override void Accept<TContext>(PdfObjectVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitNull(this, context);
    }

    public override string ToString()
    {
        return $"[Null]";
    }
}