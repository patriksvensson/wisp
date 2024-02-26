namespace Wisp.Cos;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosName : CosPrimitive, IEqualityComparer<CosName>
{
    public string Value { get; }

    public CosName(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public bool Equals(CosName? x, CosName? y)
    {
        return CosNameComparer.Shared.Equals(x, y);
    }

    public int GetHashCode(CosName obj)
    {
        return CosNameComparer.Shared.GetHashCode(obj);
    }

    public override string ToString()
    {
        return $"[Name] {Value}";
    }

    internal static class Known
    {
        public static CosName Root { get; } = new("Root");
        public static CosName Size { get; } = new("Size");
        public static CosName Info { get; } = new("Info");
        public static CosName Id { get; } = new("ID");
        public static CosName Type { get; } = new("Type");
        public static CosName Pages { get; } = new("Pages");
        public static CosName Length { get; } = new("Length");
        public static CosName Prev { get; } = new("Prev");
        public static CosName Encrypt { get; } = new("Encrypt");
        public static CosName XRefStm { get; } = new("XRefStm");
        public static CosName Kids { get; } = new("Kids");
        public static CosName Contents { get; } = new("Contents");
        public static CosName Parent { get; } = new("Parent");
        public static CosName Resources { get; } = new("Resources");
        public static CosName MediaBox { get; } = new("MediaBox");
        public static CosName Count { get; } = new("Count");
        public static CosName Title { get; } = new("Title");
        public static CosName Author { get; } = new("Author");
        public static CosName Subject { get; } = new("Subject");
        public static CosName Keywords { get; } = new("Keywords");
        public static CosName Creator { get; } = new("Creator");
        public static CosName Producer { get; } = new("Producer");
        public static CosName Filter { get; } = new("Filter");
        public static CosName DecodeParms { get; } = new("DecodeParms");
        public static CosName Predictor { get; } = new("Predictor");
        public static CosName Columns { get; } = new("Columns");
        public static CosName Colors { get; } = new("Colors");
        public static CosName BitsPerComponent { get; } = new("BitsPerComponent");
        public static CosName Index { get; } = new("Index");
        public static CosName N { get; } = new("N");
        public static CosName First { get; } = new("First");
        public static CosName W { get; } = new("W");
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