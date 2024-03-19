namespace Wisp;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosObjectId : ICosPrimitive, IEquatable<CosObjectId>, IComparable<CosObjectId>
{
    private static readonly char[] _separator = [':'];

    public int Number { get; set; }
    public int Generation { get; set; }

    public static CosObjectIdComparer Comparer => CosObjectIdComparer.Shared;

    public CosObjectId(int number, int generation)
    {
        Number = number;
        Generation = generation;
    }

    public static CosObjectId Parse(string text)
    {
        var parts = text.Split(_separator, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
        {
            return new CosObjectId(
                int.Parse(parts[0].Trim()),
                int.Parse(parts[1].Trim()));
        }

        throw new WispException("Could not parse object ID.");
    }

    public int CompareTo(CosObjectId? other)
    {
        if (other == null)
        {
            return 1;
        }

        return Number.CompareTo(other.Number);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
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

    [DebuggerStepThrough]
    public void Accept<TContext>(ICosVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitObjectId(this, context);
    }

    [DebuggerStepThrough]
    public TResult Accept<TContext, TResult>(ICosVisitor<TContext, TResult> visitor, TContext context)
    {
        return visitor.VisitObjectId(this, context);
    }

    public override string ToString()
    {
        return $"[ObjectID] {Number}:{Generation}".Trim();
    }
}

[PublicAPI]
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