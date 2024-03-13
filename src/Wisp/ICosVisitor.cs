namespace Wisp;

[PublicAPI]
public interface ICosVisitor<in TContext>
{
    void VisitArray(CosArray obj, TContext context);
    void VisitBoolean(CosBoolean obj, TContext context);
    void VisitDate(CosDate obj, TContext context);
    void VisitDictionary(CosDictionary obj, TContext context);
    void VisitHexString(CosHexString obj, TContext context);
    void VisitInteger(CosInteger obj, TContext context);
    void VisitName(CosName obj, TContext context);
    void VisitNull(CosNull obj, TContext context);
    void VisitObject(CosObject obj, TContext context);
    void VisitObjectId(CosObjectId obj, TContext context);
    void VisitObjectReference(CosObjectReference obj, TContext context);
    void VisitObjectStream(CosObjectStream obj, TContext context);
    void VisitReal(CosReal obj, TContext context);
    void VisitStream(CosStream obj, TContext context);
    void VisitString(CosString obj, TContext context);
}