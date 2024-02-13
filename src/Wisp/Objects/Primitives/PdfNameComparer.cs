namespace Wisp.Objects;

public sealed class PdfNameComparer : IEqualityComparer<PdfName>
{
    public static PdfNameComparer Shared { get; } = new();

    public bool Equals(PdfName? x, PdfName? y)
    {
        if (x == null && y == null)
        {
            return true;
        }

        if (x == null || y == null)
        {
            return false;
        }

        return x.Value.Equals(y.Value, StringComparison.Ordinal);
    }

    public int GetHashCode(PdfName obj)
    {
        return obj.Value.GetHashCode();
    }
}