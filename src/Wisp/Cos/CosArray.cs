namespace Wisp.Cos;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosArray : CosPrimitive, IEnumerable<CosPrimitive>
{
    private readonly List<CosPrimitive> _items;

    public int Count => _items.Count;

    public CosPrimitive this[int index]
    {
        get => _items[index];
    }

    public CosArray()
    {
        _items = new List<CosPrimitive>();
    }

    public void Add(CosPrimitive item)
    {
        _items.Add(item);
    }

    public CosPrimitive? GetAt(int index)
    {
        if (index >= _items.Count)
        {
            return null;
        }

        return _items[index];
    }

    public IEnumerator<CosPrimitive> GetEnumerator()
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
        where T : CosPrimitive
    {
        var obj = array.GetAt(index);
        if (obj == null)
        {
            return null;
        }

        if (obj is not T item)
        {
#if DEBUG
            throw new InvalidOperationException(
                $"Expected object at #{index} to be of type '{typeof(T).Name}', " +
                $"but it was of type '{obj.GetType().Name}'");
#else
            return null;
#endif
        }

        return item;
    }
}