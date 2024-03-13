namespace Wisp;

[PublicAPI]
public sealed class CosXRefTable : IEnumerable<CosXRef>
{
    private readonly Dictionary<CosObjectId, CosXRef> _lookup;
    private readonly List<CosXRef> _references;
    private int _highestId;

    public CosXRefTable()
    {
        _lookup = new Dictionary<CosObjectId, CosXRef>(new CosObjectIdComparer());
        _references = new List<CosXRef>();
    }

    public CosObjectId GetNextId()
    {
        return new CosObjectId(++_highestId, 0);
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

    internal CosXRefTable Merge(CosXRefTable other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var result = new CosXRefTable();
        foreach (var entry in this)
        {
            result.Add(entry);
        }

        foreach (var entry in other)
        {
            result.Add(entry);
        }

        return result;
    }

    internal bool Add(CosXRef reference)
    {
        ArgumentNullException.ThrowIfNull(reference);

        if (!_lookup.TryAdd(reference.Id, reference))
        {
            return false;
        }

        // Highest number?
        if (reference.Id.Number > _highestId)
        {
            _highestId = reference.Id.Number;
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