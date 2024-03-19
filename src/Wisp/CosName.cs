namespace Wisp;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosName : ICosPrimitive, IEquatable<CosName>
{
    public string Value { get; }

    public static CosNameComparer Comparer => CosNameComparer.Shared;

    public CosName(string value)
    {
        Value = value.TrimStart('/') ?? throw new ArgumentNullException(nameof(value));
    }

    public bool Equals(CosName? other)
    {
        return CosNameComparer.Shared.Equals(this, other);
    }

    public override int GetHashCode()
    {
        return CosNameComparer.Shared.GetHashCode(this);
    }

    [DebuggerStepThrough]
    public void Accept<TContext>(ICosVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitName(this, context);
    }

    [DebuggerStepThrough]
    public TResult Accept<TContext, TResult>(ICosVisitor<TContext, TResult> visitor, TContext context)
    {
        return visitor.VisitName(this, context);
    }

    public override string ToString()
    {
        return $"[Name] {Value}";
    }
}

public sealed class CosNameComparer : IEqualityComparer<CosName>
{
    public static CosNameComparer Shared { get; } = new();

    public bool Equals(CosName? x, CosName? y)
    {
        if (x == null && y == null)
        {
            return true;
        }

        if (x == null || y == null)
        {
            return false;
        }

        return x.Value.Equals(y.Value, StringComparison.Ordinal);
    }

    public int GetHashCode(CosName obj)
    {
        return obj.Value.GetHashCode();
    }
}