namespace Wisp;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PdfName : PdfObject, IEqualityComparer<PdfName>
{
    public string Value { get; }

    public PdfName(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public bool Equals(PdfName? x, PdfName? y)
    {
        return PdfNameComparer.Shared.Equals(x, y);
    }

    public int GetHashCode(PdfName obj)
    {
        return PdfNameComparer.Shared.GetHashCode(obj);
    }

    public override string ToString()
    {
        return $"[Name] {Value}";
    }

    internal static class Known
    {
        public static PdfName Root { get; } = new("Root");
        public static PdfName Size { get; } = new("Size");
        public static PdfName Info { get; } = new("Info");
        public static PdfName Type { get; } = new("Type");
        public static PdfName Pages { get; } = new("Pages");
        public static PdfName Length { get; } = new("Length");
        public static PdfName Prev { get; } = new("Prev");
        public static PdfName XRefStm { get; } = new("XRefStm");
        public static PdfName Kids { get; } = new("Kids");
        public static PdfName Contents { get; } = new("Contents");
        public static PdfName Parent { get; } = new("Parent");
        public static PdfName Resources { get; } = new("Resources");
        public static PdfName MediaBox { get; } = new("MediaBox");
        public static PdfName Count { get; } = new("Count");
        public static PdfName Title { get; } = new("Title");
        public static PdfName Author { get; } = new("Author");
        public static PdfName Subject { get; } = new("Subject");
        public static PdfName Keywords { get; } = new("Keywords");
        public static PdfName Creator { get; } = new("Creator");
        public static PdfName Producer { get; } = new("Producer");
        public static PdfName Filter { get; } = new("Filter");
        public static PdfName DecodeParms { get; } = new("DecodeParms");
        public static PdfName Predictor { get; } = new("Predictor");
        public static PdfName Columns { get; } = new("Columns");
        public static PdfName Colors { get; } = new("Colors");
        public static PdfName BitsPerComponent { get; } = new("BitsPerComponent");
        public static PdfName Index { get; } = new("Index");
        public static PdfName N { get; } = new("N");
        public static PdfName First { get; } = new("First");
        public static PdfName W { get; } = new("W");
    }
}