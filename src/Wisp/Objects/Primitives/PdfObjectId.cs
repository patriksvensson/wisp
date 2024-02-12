namespace Wisp;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PdfObjectId : PdfObject, IEquatable<PdfObjectId>
{
    public int Number { get; set; }
    public int Generation { get; set; }

    public PdfObjectId(int number, int generation)
    {
        Number = number;
        Generation = generation;
    }

    public static PdfObjectId Parse(string text)
    {
        var parts = text.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
        {
            return new PdfObjectId(
                int.Parse(parts[0].Trim()),
                int.Parse(parts[1].Trim()));
        }

        throw new InvalidOperationException("Could not parse object ID.");
    }

    public override bool Equals(object? obj)
    {
        if (obj is PdfObjectId objectId)
        {
            return Equals(objectId);
        }

        return base.Equals(obj);
    }

    public bool Equals(PdfObjectId? other)
    {
        return PdfObjectIdComparer.Shared.Equals(this, other);
    }

    public override void Accept<TContext>(PdfObjectVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitObjectId(this, context);
    }

    public override int GetHashCode()
    {
        return PdfObjectIdComparer.Shared.GetHashCode(this);
    }

    public override string ToString()
    {
        return $"[ObjectID] {Number}:{Generation}".Trim();
    }
}