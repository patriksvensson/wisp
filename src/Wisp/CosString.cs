namespace Wisp;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosString : ICosPrimitive
{
    public string Value { get; }
    public CosStringEncoding Encoding { get; }

    public CosString(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Encoding = CosStringEncoding.Raw;
    }

    public CosString(string value, CosStringEncoding encoding)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Encoding = encoding;
    }

    [DebuggerStepThrough]
    public void Accept<TContext>(ICosVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitString(this, context);
    }

    public override string ToString()
    {
        return $"[String] {Value} ({Encoding})";
    }
}