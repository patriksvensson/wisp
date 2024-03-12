using System.Runtime.CompilerServices;

namespace Wisp;

[PublicAPI]
public abstract class CosVisitor<TContext> : ICosVisitor<TContext>
{
    public virtual void VisitArray(CosArray obj, TContext context)
    {
    }

    public virtual void VisitBoolean(CosBoolean obj, TContext context)
    {
    }

    public virtual void VisitDate(CosDate obj, TContext context)
    {
    }

    public virtual void VisitDictionary(CosDictionary obj, TContext context)
    {
    }

    public virtual void VisitHexString(CosHexString obj, TContext context)
    {
    }

    public virtual void VisitInteger(CosInteger obj, TContext context)
    {
    }

    public virtual void VisitName(CosName obj, TContext context)
    {
    }

    public virtual void VisitNull(CosNull obj, TContext context)
    {
    }

    public virtual void VisitObject(CosObject obj, TContext context)
    {
    }

    public virtual void VisitObjectId(CosObjectId obj, TContext context)
    {
    }

    public virtual void VisitObjectReference(CosObjectReference obj, TContext context)
    {
    }

    public virtual void VisitObjectStream(CosObjectStream obj, TContext context)
    {
    }

    public virtual void VisitReal(CosReal obj, TContext context)
    {
    }

    public virtual void VisitStream(CosStream obj, TContext context)
    {
    }

    public virtual void VisitString(CosString obj, TContext context)
    {
    }

    [DebuggerStepThrough]
    protected virtual void Visit(ICosVisitable? obj, TContext context)
    {
        if (obj == null)
        {
            return;
        }

        RuntimeHelpers.EnsureSufficientExecutionStack();
        obj.Accept(this, context);
    }

    [DebuggerStepThrough]
    protected void VisitMany(IEnumerable<ICosPrimitive> syntaxes, TContext context)
    {
        foreach (var node in syntaxes)
        {
            Visit(node, context);
        }
    }
}