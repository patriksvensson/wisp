namespace Wisp.Cos;

public abstract class CosXRef
{
    public CosObjectId Id { get; }

    protected CosXRef(CosObjectId id)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
    }
}

[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosFreeXRef : CosXRef
{
    public CosFreeXRef(CosObjectId id)
        : base(id)
    {
    }

    public override string ToString()
    {
        return $"[XRef] {Id.Number}:{Id.Generation} (free)";
    }
}

[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosIndirectXRef : CosXRef
{
    public int Position { get; }

    public CosIndirectXRef(CosObjectId id, int position)
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
public sealed class CosStreamXRef : CosXRef
{
    public CosObjectId StreamId { get; }
    public int Index { get; }

    public CosStreamXRef(CosObjectId id, CosObjectId streamId, int index)
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