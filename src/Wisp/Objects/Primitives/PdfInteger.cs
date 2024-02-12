namespace Wisp;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PdfInteger : PdfObject
{
    public int Value { get; }

    public PdfInteger(int value)
    {
        Value = value;
    }

    public override void Accept<TContext>(PdfObjectVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitInteger(this, context);
    }

    public override string ToString()
    {
        return $"[Integer] {Value}";
    }
}