namespace Wisp.Cli.Utilities;

public static class CosTypeNameResolver
{
    public static string GetName(ICosPrimitive primitive)
    {
        return primitive.Accept(Visitor.Shared, null);
    }

    private sealed class Visitor : CosVisitor<object?, string>
    {
        public static Visitor Shared { get; } = new();

        public override string VisitArray(CosArray obj, object? context)
        {
            return "Array";
        }

        public override string VisitBoolean(CosBoolean obj, object? context)
        {
            return "Boolean";
        }

        public override string VisitDate(CosDate obj, object? context)
        {
            return "Date";
        }

        public override string VisitDictionary(CosDictionary obj, object? context)
        {
            return "Dictionary";
        }

        public override string VisitHexString(CosHexString obj, object? context)
        {
            return "Hex String";
        }

        public override string VisitInteger(CosInteger obj, object? context)
        {
            return "Integer";
        }

        public override string VisitName(CosName obj, object? context)
        {
            return "Name";
        }

        public override string VisitNull(CosNull obj, object? context)
        {
            return "Null";
        }

        public override string VisitObject(CosObject obj, object? context)
        {
            return "Object";
        }

        public override string VisitObjectId(CosObjectId obj, object? context)
        {
            return "Object ID";
        }

        public override string VisitObjectReference(CosObjectReference obj, object? context)
        {
            return "Object Reference";
        }

        public override string VisitObjectStream(CosObjectStream obj, object? context)
        {
            return "Object Stream";
        }

        public override string VisitReal(CosReal obj, object? context)
        {
            return "Real";
        }

        public override string VisitStream(CosStream obj, object? context)
        {
            return "Stream";
        }

        public override string VisitString(CosString obj, object? context)
        {
            return "String";
        }
    }
}