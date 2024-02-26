namespace Wisp.Cos;

public sealed class CosXRefTable : IEnumerable<CosXRef>
{
    private readonly Dictionary<CosObjectId, CosXRef> _lookup;
    private readonly List<CosXRef> _references;

    public CosXRefTable()
    {
        _lookup = new Dictionary<CosObjectId, CosXRef>(new CosObjectIdComparer());
        _references = new List<CosXRef>();
    }

    public CosXRef? GetXRef(CosObjectId key)
    {
        return _lookup.GetValueOrDefault(key);
    }

    public bool Contains(CosObjectId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _lookup.ContainsKey(id);
    }

    public CosXRefTable Merge(CosXRefTable other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var result = new CosXRefTable();
        foreach (var entry in this)
        {
            result.Add(entry);
        }

        if (other.Any(entry => !result.Add(entry)))
        {
            throw new InvalidOperationException("Could not add xref entry to xref table");
        }

        return result;
    }

    public bool Add(CosXRef reference)
    {
        ArgumentNullException.ThrowIfNull(reference);

        if (!_lookup.TryAdd(reference.Id, reference))
        {
            return false;
        }

        _references.Add(reference);
        return true;
    }

    public IEnumerator<CosXRef> GetEnumerator()
    {
        return _references.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public static class CosXRefTableExtensions
{
    public static CosXRef GetRequiredXRef(this CosXRefTable table, CosObjectId id)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(id);

        var xref = table.GetXRef(id);
        if (xref == null)
        {
            throw new InvalidOperationException(
                $"Could not find xref {id.Number}:{id.Generation} in xref table.");
        }

        return xref;
    }
}