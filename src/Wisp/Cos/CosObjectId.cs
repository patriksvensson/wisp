namespace Wisp.Cos;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosObjectId : CosPrimitive, IEquatable<CosObjectId>
{
    public int Number { get; set; }
    public int Generation { get; set; }

    public CosObjectId(int number, int generation)
    {
        Number = number;
        Generation = generation;
    }

    public static CosObjectId Parse(string text)
    {
        var parts = text.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
        {
            return new CosObjectId(
                int.Parse(parts[0].Trim()),
                int.Parse(parts[1].Trim()));
        }

        throw new InvalidOperationException("Could not parse object ID.");
    }

    public override bool Equals(object? obj)
    {
        if (object.ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is CosObjectId objectId)
        {
            return Equals(objectId);
        }

        return false;
    }

    public bool Equals(CosObjectId? other)
    {
        return CosObjectIdComparer.Shared.Equals(this, other);
    }

    public override int GetHashCode()
    {
        return CosObjectIdComparer.Shared.GetHashCode(this);
    }

    public override string ToString()
    {
        return $"[ObjectID] {Number}:{Generation}".Trim();
    }
}

public sealed class CosObjectIdComparer : IEqualityComparer<CosObjectId>
{
    public static CosObjectIdComparer Shared { get; } = new();

    public bool Equals(CosObjectId? x, CosObjectId? y)
    {
        if (x == null && y == null)
        {
            return true;
        }

        if (x == null || y == null)
        {
            return false;
        }

        return x.Number == y.Number &&
               x.Generation == y.Generation;
    }

    public int GetHashCode(CosObjectId obj)
    {
        return HashCode.Combine(obj.Number, obj.Generation);
    }
}