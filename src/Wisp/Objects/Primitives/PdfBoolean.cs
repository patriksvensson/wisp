namespace Wisp;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PdfBoolean : PdfObject
{
    public bool Value { get; }

    public PdfBoolean(bool value)
    {
        Value = value;
    }

    public override void Accept<TContext>(PdfObjectVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitBoolean(this, context);
    }

    public override string ToString()
    {
        return $"[Boolean] {Value}";
    }
}