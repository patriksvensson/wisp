namespace Wisp.Cos;

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

    public CosObject? Get(CosObjectId id, CosResolveFlags flags = CosResolveFlags.None)
    {
        var shouldInvalidate = flags.HasFlag(CosResolveFlags.Invalidate);
        if (!shouldInvalidate)
        {
            // Try get the object from caches
            if (_objects.TryGetValue(id, out var obj))
            {
                return obj;
            }
        }

        // Should we try to resolve the object from the
        // PDF document stream?
        var shouldResolve = !flags.HasFlag(CosResolveFlags.NoResolve);
        if (shouldResolve && _resolver != null)
        {
            var obj = _resolver?.GetObject(this, id);
            if (obj == null)
            {
                return null;
            }

            // Should we add the resolved object to the cache?
            var shouldCache = !flags.HasFlag(CosResolveFlags.NoCache);
            if (shouldCache)
            {
                _objects.TryAdd(id, obj);
            }

            return obj;
        }

        return null;
    }

    public void Set(CosObject obj)
    {
        _objects[obj.Id] = obj;

        if (!_table.Contains(obj.Id))
        {
            _table.Add(new CosIndirectXRef(obj.Id));
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
                    var obj = _collection.Get(current.Id, CosResolveFlags.NoCache);
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

[Flags]
[PublicAPI]
public enum CosResolveFlags
{
    None = 0,

    /// <summary>
    /// If an objects is cached, skip the cache
    /// completely when retrieving it.
    /// Can only be used when working with a loaded document.
    /// </summary>
    Invalidate = 1 << 0,

    /// <summary>
    /// An resolved objects will not be added
    /// to the object cache.
    /// Can only be used when working with a loaded document.
    /// </summary>
    NoCache = 1 << 1,

    /// <summary>
    /// If the document is loaded, the object will not
    /// be resolved from the document stream.
    /// </summary>
    NoResolve = 1 << 2,
}

public interface ICosObjectCache : IEnumerable<CosObject>
{
    CosObject? Get(CosObjectId id, CosResolveFlags flags = CosResolveFlags.None);
    void Set(CosObject obj);
}

[PublicAPI]
public static class ICosObjectCacheExtensions
{
    public static CosObject? Get(this ICosObjectCache collection, int number, int generation, CosResolveFlags flags = CosResolveFlags.None)
    {
        return collection.Get(new CosObjectId(number, generation), flags);
    }

    public static CosObject? Get(this ICosObjectCache collection, CosObjectReference reference, CosResolveFlags flags = CosResolveFlags.None)
    {
        return collection.Get(new CosObjectId(reference.Id.Number, reference.Id.Generation), flags);
    }
}