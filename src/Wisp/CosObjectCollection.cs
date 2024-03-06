namespace Wisp.Cos;

[PublicAPI]
public sealed class CosObjectCollection
{
    private readonly CosXRefTable _table;
    private readonly CosObjectResolver? _resolver;
    private readonly Dictionary<CosObjectId, CosObject> _objects;

    public CosObjectCollection(CosXRefTable table, CosObjectResolver? resolver)
    {
        _table = table ?? throw new ArgumentNullException(nameof(table));
        _resolver = resolver;
        _objects = new Dictionary<CosObjectId, CosObject>(CosObjectIdComparer.Shared);
    }

    public CosObject? GetById(CosObjectId id)
    {
        if (_objects.TryGetValue(id, out var obj))
        {
            return obj;
        }

        obj = _resolver?.GetObject(id);
        if (obj == null)
        {
            return null;
        }

        // Add the object to the cache
        _objects.Add(id, obj);
        return obj;
    }
}

[PublicAPI]
public static class CosObjectCollectionExtensions
{
    public static CosObject? GetById(this CosObjectCollection collection, int number, int generation)
    {
        return collection.GetById(new CosObjectId(number, generation));
    }
}