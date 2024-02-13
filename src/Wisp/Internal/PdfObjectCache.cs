namespace Wisp.Internal;

internal sealed class PdfObjectCache
{
    public void AddOrUpdate(PdfObject obj)
    {
        // For now, do nothing.
    }

    public bool TryGetObject(PdfObjectId id, [NotNullWhen(true)] out PdfObject? result)
    {
        // For now, do nothing.
        result = null;
        return false;
    }
}