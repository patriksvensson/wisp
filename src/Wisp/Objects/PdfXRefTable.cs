namespace Wisp;

public sealed class PdfXRefTable : PdfObject, IEnumerable<PdfXRef>
{
    private readonly Dictionary<PdfObjectId, PdfXRef> _lookup;
    private readonly List<PdfXRef> _references;

    public PdfXRefTable()
    {
        _lookup = new Dictionary<PdfObjectId, PdfXRef>(new PdfObjectIdComparer());
        _references = new List<PdfXRef>();
    }

    public PdfXRef? GetReference(PdfObjectId key)
    {
        if (_lookup.TryGetValue(key, out var value))
        {
            return value;
        }

        return null;
    }

    public bool Contains(PdfObjectId id)
    {
        if (id is null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        return _lookup.ContainsKey(id);
    }

    public void Merge(PdfXRefTable table)
    {
        if (table is null)
        {
            throw new ArgumentNullException(nameof(table));
        }

        foreach (var entry in table)
        {
            Add(entry);
        }
    }

    public void Add(PdfXRef reference)
    {
        if (reference is null)
        {
            throw new ArgumentNullException(nameof(reference));
        }

        if (_lookup.ContainsKey(reference.Id))
        {
            return;
        }

        _lookup[reference.Id] = reference;
        _references.Add(reference);
    }

    public IEnumerator<PdfXRef> GetEnumerator()
    {
        return _references.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}