namespace Wisp;

public abstract class PdfXRef
{
    public PdfObjectId Id { get; }

    protected PdfXRef(PdfObjectId id)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
    }
}

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PdfFreeXRef : PdfXRef
{
    public PdfFreeXRef(PdfObjectId id)
        : base(id)
    {
    }

    public override string ToString()
    {
        return $"[XRef] {Id.Number}:{Id.Generation} (free)";
    }
}

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PdfIndirectXRef : PdfXRef
{
    public int Position { get; }

    public PdfIndirectXRef(PdfObjectId id, int position)
        : base(id)
    {
        Position = position;
    }

    public override string ToString()
    {
        return $"[XRef] {Id.Number}:{Id.Generation} Position = {Position}";
    }
}