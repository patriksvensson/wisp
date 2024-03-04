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

    public bool TryGetValue(int index, [NotNullWhen(true)] out CosPrimitive? item)
    {
        if (index >= _items.Count)
        {
            item = null;
            return false;
        }

        item = _items[index];
        return true;
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
    public static bool TryGetValue<T>(this CosArray array, int index, [NotNullWhen(true)] out T? item)
        where T : CosPrimitive
    {
        if (!array.TryGetValue(index, out var primitive) || primitive is not T result)
        {
            item = null;
            return false;
        }

        item = result;
        return true;
    }

    public static T? GetOptional<T>(this CosArray array, int index)
        where T : CosPrimitive
    {
        TryGetValue<T>(array, index, out var result);
        return result;
    }

    public static T GetRequired<T>(this CosArray array, int index)
        where T : CosPrimitive
    {
        if (!array.TryGetValue(index, out var primitive))
        {
            throw new IndexOutOfRangeException();
        }

        if (primitive is not T result)
        {
            throw new InvalidOperationException(
                $"Expected item at position {index} to be " +
                $"of type '{typeof(T).Name}', but was " +
                $"'{primitive.GetType().Name}'");
        }

        return result;
    }
}