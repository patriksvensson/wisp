namespace Wisp.Objects;

public abstract class PdfObject
{
    public abstract void Accept<TContext>(PdfObjectVisitor<TContext> visitor, TContext context);
}