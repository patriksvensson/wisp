namespace Wisp.Cos;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosName : CosPrimitive, IEquatable<CosName>
{
    public string Value { get; }

    public CosName(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public bool Equals(CosName? other)
    {
        return CosNameComparer.Shared.Equals(this, other);
    }

    public override int GetHashCode()
    {
        return CosNameComparer.Shared.GetHashCode(this);
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