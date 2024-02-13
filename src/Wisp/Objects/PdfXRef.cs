namespace Wisp.Objects;

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

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PdfStreamXRef : PdfXRef
{
    public PdfObjectId StreamId { get; }
    public int Index { get; }

    public PdfStreamXRef(PdfObjectId id, PdfObjectId streamId, int index)
        : base(id)
    {
        StreamId = streamId ?? throw new ArgumentNullException(nameof(streamId));
        Index = index;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var streamId = $"{StreamId.Number}:{StreamId.Generation}";
        return $"[XRef] {Id.Number}:{Id.Generation} Stream = {streamId}, Index = {Index}";
    }
}