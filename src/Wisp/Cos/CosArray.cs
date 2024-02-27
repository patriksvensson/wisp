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