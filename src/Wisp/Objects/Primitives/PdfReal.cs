namespace Wisp;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PdfReal : PdfObject
{
    public double Value { get; }

    public PdfReal(double value)
    {
        Value = value;
    }

    public override void Accept<TContext>(PdfObjectVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitReal(this, context);
    }

    public override string ToString()
    {
        return $"[Real] {Value}";
    }
}