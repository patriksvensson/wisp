namespace Wisp.Cos;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosString : CosPrimitive
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

public enum CosStringEncoding
{
    Raw = 0,
    Unicode = 1,
    BigEndianUnicode = 2,
    HexLiteral = 3,
}