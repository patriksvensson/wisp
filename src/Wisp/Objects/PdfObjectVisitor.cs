namespace Wisp.Objects;

public abstract class PdfObjectVisitor<TContext>
{
    public virtual void VisitArray(PdfArray pdfArray, TContext context)
    {
    }

    public virtual void VisitBoolean(PdfBoolean pdfBoolean, TContext context)
    {
    }

    public virtual void VisitDictionary(PdfDictionary pdfDictionary, TContext context)
    {
    }

    public virtual void VisitInteger(PdfInteger pdfInteger, TContext context)
    {
    }

    public virtual void VisitName(PdfName pdfName, TContext context)
    {
    }

    public virtual void VisitNull(PdfNull pdfNull, TContext context)
    {
    }

    public virtual void VisitObjectId(PdfObjectId pdfObjectId, TContext context)
    {
    }

    public virtual void VisitObjectDefinition(PdfObjectDefinition pdfObjectDefinition, TContext context)
    {
    }

    public virtual void VisitObjectStream(PdfObjectStream pdfObjectStream, TContext context)
    {
    }

    public virtual void VisitReal(PdfReal pdfReal, TContext context)
    {
    }

    public virtual void VisitStream(PdfStream pdfStream, TContext context)
    {
    }

    public virtual void VisitString(PdfString pdfString, TContext context)
    {
    }
}