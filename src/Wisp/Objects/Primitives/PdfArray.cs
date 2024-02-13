namespace Wisp.Objects;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PdfArray : PdfObject, IEnumerable<PdfObject>
{
    private readonly List<PdfObject> _items;

    public int Count => _items.Count;

    public PdfObject this[int index]
    {
        get => _items[index];
    }

    public PdfArray()
    {
        _items = new List<PdfObject>();
    }

    public void Add(PdfObject item)
    {
        _items.Add(item);
    }

    public IEnumerator<PdfObject> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override void Accept<TContext>(PdfObjectVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitArray(this, context);
    }

    public override string ToString()
    {
        return $"[Array] Count = {_items.Count}";
    }
}