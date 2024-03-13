namespace Wisp;

[PublicAPI]
public abstract class CosXRef
{
    public CosObjectId Id { get; }

    protected CosXRef(CosObjectId id)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
    }

    public abstract CosXRef CreateCopy();
}

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosIndirectXRef : CosXRef
{
    public long? Position { get; set; }

    public CosIndirectXRef(CosObjectId id)
        : base(id)
    {
    }

    public CosIndirectXRef(CosObjectId id, long position)
        : base(id)
    {
        Position = position;
    }

    public override CosXRef CreateCopy()
    {
        if (Position != null)
        {
            return new CosIndirectXRef(Id, Position.Value);
        }
        else
        {
            return new CosIndirectXRef(Id);
        }
    }

    public override string ToString()
    {
        return $"[XRef] {Id.Number}:{Id.Generation} Position = {Position}";
    }
}

[PublicAPI]
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

    public override CosXRef CreateCopy()
    {
        return new CosStreamXRef(Id, StreamId, Index);
    }

    public override string ToString()
    {
        var streamId = $"{StreamId.Number}:{StreamId.Generation}";
        return $"[XRef] {Id.Number}:{Id.Generation} Stream = {streamId}, Index = {Index}";
    }
}