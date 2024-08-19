namespace Wisp;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosString : ICosPrimitive
{
    public string Value { get; }
    public CosStringEncoding Encoding { get; set; }

    public CosString(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Encoding = value.All(char.IsAscii) ? CosStringEncoding.Ascii : CosStringEncoding.BigEndianUnicode;
    }

    internal CosString(string value, CosStringEncoding encoding)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Encoding = encoding;
    }

    [DebuggerStepThrough]
    public void Accept<TContext>(ICosVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitString(this, context);
    }

    [DebuggerStepThrough]
    public TResult Accept<TContext, TResult>(ICosVisitor<TContext, TResult> visitor, TContext context)
    {
        return visitor.VisitString(this, context);
    }

    public override string ToString()
    {
        return $"[String] {Value} ({Encoding})";
    }
}