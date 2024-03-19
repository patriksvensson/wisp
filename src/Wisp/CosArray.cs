namespace Wisp;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosArray : ICosPrimitive, IEnumerable<ICosPrimitive>
{
    private readonly List<ICosPrimitive> _items;

    public int Count => _items.Count;

    public ICosPrimitive this[int index]
    {
        get => _items[index];
    }

    public CosArray()
    {
        _items = new List<ICosPrimitive>();
    }

    public CosArray(IEnumerable<ICosPrimitive> items)
        : this()
    {
        foreach (var item in items)
        {
            Add(item);
        }
    }

    public void Add(ICosPrimitive item)
    {
        _items.Add(item);
    }

    public ICosPrimitive? GetAt(int index)
    {
        if (index >= _items.Count)
        {
            return null;
        }

        return _items[index];
    }

    public IEnumerator<ICosPrimitive> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        return $"[Array] Count = {_items.Count}";
    }

    [DebuggerStepThrough]
    public void Accept<TContext>(ICosVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitArray(this, context);
    }

    [DebuggerStepThrough]
    public TResult Accept<TContext, TResult>(ICosVisitor<TContext, TResult> visitor, TContext context)
    {
        return visitor.VisitArray(this, context);
    }
}

[PublicAPI]
public static class CosArrayExtensions
{
    public static CosInteger? GetIntegerAt(this CosArray array, int index)
    {
        return array.GetAt<CosInteger>(index);
    }

    public static CosName? GetNameAt(this CosArray array, int index)
    {
        return array.GetAt<CosName>(index);
    }

    public static CosObjectId? GetObjectIdAt(this CosArray array, int index)
    {
        return array.GetAt<CosObjectId>(index);
    }

    public static CosDictionary? GetDictionaryAt(this CosArray array, int index)
    {
        return array.GetAt<CosDictionary>(index);
    }

    public static CosArray? GetArrayAt(this CosArray array, int index)
    {
        return array.GetAt<CosArray>(index);
    }

    public static int? GetInt32At(this CosArray array, int index)
    {
        return array.GetAt<CosInteger>(index)?.IntValue;
    }

    public static long? GetInt64At(this CosArray array, int index)
    {
        return array.GetAt<CosInteger>(index)?.Value;
    }

    public static T? GetAt<T>(this CosArray array, int index)
        where T : ICosPrimitive
    {
        var obj = array.GetAt(index);
        if (obj == null)
        {
            return default;
        }

        if (obj is not T item)
        {
#if DEBUG
            throw new WispException(
                $"Expected object at #{index} to be of type '{typeof(T).Name}', " +
                $"but it was of type '{obj.GetType().Name}'");
#else
            return default;
#endif
        }

        return item;
    }
}