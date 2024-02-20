namespace Wisp.Objects;

public abstract class PdfObjectVisitor<TContext>
{
    public virtual void VisitArray(PdfArray obj, TContext context)
    {
    }

    public virtual void VisitBoolean(PdfBoolean obj, TContext context)
    {
    }

    public virtual void VisitDate(PdfDate obj, TContext context)
    {
    }

    public virtual void VisitDictionary(PdfDictionary obj, TContext context)
    {
    }

    public virtual void VisitInteger(PdfInteger obj, TContext context)
    {
    }

    public virtual void VisitName(PdfName obj, TContext context)
    {
    }

    public virtual void VisitNull(PdfNull obj, TContext context)
    {
    }

    public virtual void VisitObjectId(PdfObjectId obj, TContext context)
    {
    }

    public virtual void VisitObjectDefinition(PdfObjectDefinition obj, TContext context)
    {
    }

    public virtual void VisitObjectStream(PdfObjectStream obj, TContext context)
    {
    }

    public virtual void VisitReal(PdfReal obj, TContext context)
    {
    }

    public virtual void VisitStream(PdfStream obj, TContext context)
    {
    }

    public virtual void VisitString(PdfString obj, TContext context)
    {
    }
}