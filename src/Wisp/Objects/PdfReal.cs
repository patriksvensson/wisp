namespace Wisp;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PdfReal : PdfObject
{
    public double Value { get; }

    public PdfReal(double value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return $"[Real] {Value}";
    }
}