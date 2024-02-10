namespace Wisp;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PdfBoolean : PdfObject
{
    public bool Value { get; }

    public PdfBoolean(bool value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return $"[Boolean] {Value}";
    }
}