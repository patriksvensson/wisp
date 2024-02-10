namespace Wisp;

public sealed class PdfObjectIdComparer : IEqualityComparer<PdfObjectId>
{
    public static PdfObjectIdComparer Shared { get; } = new();

    public bool Equals(PdfObjectId? x, PdfObjectId? y)
    {
        if (x == null && y == null)
        {
            return true;
        }

        if (x == null || y == null)
        {
            return false;
        }

        return x.Number == y.Number &&
               x.Generation == y.Generation;
    }

    public int GetHashCode(PdfObjectId obj)
    {
        return HashCode.Combine(obj.Number, obj.Generation);
    }
}