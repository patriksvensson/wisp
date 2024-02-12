namespace Wisp;

public sealed class PdfObjectDefinition : PdfObject
{
    public PdfObjectId Id { get; }
    public PdfObject Object { get; }

    public PdfObjectDefinition(PdfObjectId id, PdfObject obj)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Object = obj ?? throw new ArgumentNullException(nameof(obj));
    }

    public override void Accept<TContext>(PdfObjectVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitObjectDefinition(this, context);
    }
}