namespace Wisp.Cos;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosString : ICosPrimitive
{
    public string Value { get; }
    public CosStringEncoding Encoding { get; }

    public CosString(string value, CosStringEncoding encoding)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Encoding = encoding;
    }

    public override string ToString()
    {
        return $"[String] {Value} ({Encoding})";
    }
}