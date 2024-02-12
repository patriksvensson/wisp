namespace Wisp;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PdfString : PdfObject
{
    public string Value { get; }
    public PdfStringEncoding Encoding { get; }

    public PdfString(string value, PdfStringEncoding encoding)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Encoding = encoding;
    }

    public override void Accept<TContext>(PdfObjectVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitString(this, context);
    }

    public override string ToString()
    {
        return $"[String] {Value} ({Encoding})";
    }
}