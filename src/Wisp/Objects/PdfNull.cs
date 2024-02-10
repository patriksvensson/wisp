namespace Wisp;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PdfNull : PdfObject
{
    public override string ToString()
    {
        return $"[Null]";
    }
}