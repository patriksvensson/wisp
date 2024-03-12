namespace Wisp.Cos;

internal sealed class CosObjectCacheFrontend : ICosObjectCache, IEnumerable<CosObject>
{
    private readonly CosObjectCache _cache;

    public CosObjectCacheFrontend(CosObjectCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public CosObject? Get(CosObjectId id, bool resolve = true, bool cache = true)
    {
        return _cache.Get(id, cache);
    }

    public void Set(CosObject obj)
    {
        _cache.Set(obj);
    }

    public IEnumerator<CosObject> GetEnumerator()
    {
        return _cache.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

internal sealed class CosObjectCache : ICosObjectCache
{
    private readonly CosXRefTable _table;
    private readonly CosObjectResolver? _resolver;
    private readonly Dictionary<CosObjectId, CosObject> _objects;

    public CosObjectCache(CosXRefTable table, CosObjectResolver? resolver)
    {
        _table = table ?? throw new ArgumentNullException(nameof(table));
        _resolver = resolver;
        _objects = new Dictionary<CosObjectId, CosObject>(CosObjectIdComparer.Shared);
    }

    public CosObject? Get(CosObjectId id, bool resolve = true, bool cache = true)
    {
        if (_objects.TryGetValue(id, out var obj))
        {
            return obj;
        }

        if (resolve)
        {
            obj = _resolver?.GetObject(this, id);
            if (obj == null)
            {
                return null;
            }

            if (cache)
            {
                // Add the object to the cache
                _objects.Add(id, obj);
            }
        }

        return obj;
    }

    public void Set(CosObject obj)
    {
        _objects[obj.Id] = obj;

        if (!_table.Contains(obj.Id))
        {
            _table.Add(new CosIndirectXRef(obj.Id, -1));
        }
    }

    public IEnumerator<CosObject> GetEnumerator()
    {
        return new Enumerator(this, _table);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private sealed class Enumerator : IEnumerator<CosObject>
    {
        private readonly CosObjectCache _collection;
        private readonly CosXRefTable _table;
        private IEnumerator<CosXRef> _source;
        private CosObject? _current;

        public CosObject Current
        {
            get
            {
                if (_current == null)
                {
                    throw new InvalidOperationException("Current cannot be used in the current state");
                }

                return _current;
            }
        }

        object IEnumerator.Current => Current;

        public Enumerator(
            CosObjectCache collection,
            CosXRefTable table)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            _table = table ?? throw new ArgumentNullException(nameof(table));
            _source = _table.GetEnumerator();
        }

        public void Dispose()
        {
            _source.Dispose();
        }

        public void Reset()
        {
            _source.Dispose();
            _source = _table.GetEnumerator();
        }

        public bool MoveNext()
        {
            while (true)
            {
                if (!_source.MoveNext())
                {
                    return false;
                }

                var current = _source.Current;
                if (current is CosIndirectXRef)
                {
                    var obj = _collection.Get(current.Id, cache: false);
                    if (obj != null)
                    {
                        _current = obj;
                        return true;
                    }
                }
            }
        }
    }
}

public interface ICosObjectCache : IEnumerable<CosObject>
{
    CosObject? Get(CosObjectId id, bool resolve = true, bool cache = true);
    void Set(CosObject obj);
}

[PublicAPI]
public static class CosObjectCollectionExtensions
{
    public static CosObject? Get(this ICosObjectCache collection, int number, int generation, bool resolve = true, bool cache = true)
    {
        return collection.Get(new CosObjectId(number, generation), resolve, cache);
    }

    public static CosObject? Get(this ICosObjectCache collection, CosDocument owner, int number, int generation,
        bool resolve = true, bool cache = true)
    {
        return collection.Get(new CosObjectId(number, generation), resolve, cache);
    }

    public static CosObject? Get(this ICosObjectCache collection, CosObjectReference reference, bool resolve = true, bool cache = true)
    {
        return collection.Get(new CosObjectId(reference.Id.Number, reference.Id.Generation), resolve, cache);
    }
}