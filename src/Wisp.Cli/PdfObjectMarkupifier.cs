using System.Text;

namespace Wisp.Cli;

public static class PdfObjectMarkupifier
{
    private static Visitor _visitor = new Visitor();

    public static string ToMarkup(PdfObject obj)
    {
        var builder = new StringBuilder();
        obj.Accept(_visitor, builder);
        return builder.ToString();
    }

    public sealed class Visitor : PdfObjectVisitor<StringBuilder>
    {
        public override void VisitArray(PdfArray obj, StringBuilder context)
        {
            context.Append($"[silver][[Array]][/] [green]{obj.Count}[/] [silver]item(s)[/]");
        }

        public override void VisitBoolean(PdfBoolean obj, StringBuilder context)
        {
            context.Append("[silver][[Boolean]][/]" + (obj.Value ? "[yellow]true[/]" : "[yellow]false[/]"));
        }

        public override void VisitDictionary(PdfDictionary obj, StringBuilder context)
        {
            context.Append($"[silver][[Dictionary]][/] [green]{obj.Count}[/] [silver]item(s)[/]");
        }

        public override void VisitInteger(PdfInteger obj, StringBuilder context)
        {
            context.Append($"[silver][[Integer]][/] [yellow]{obj.Value}[/]");
        }

        public override void VisitName(PdfName obj, StringBuilder context)
        {
            context.Append($"[silver]/[/][yellow]{obj.Value}[/]");
        }

        public override void VisitNull(PdfNull obj, StringBuilder context)
        {
            context.Append("[silver][[Null]][/]");
        }

        public override void VisitObjectDefinition(PdfObjectDefinition obj, StringBuilder context)
        {
            var id = PdfObjectMarkupifier.ToMarkup(obj.Id);
            var value = PdfObjectMarkupifier.ToMarkup(obj.Object);
            context.Append($"{id} -> {value}");
        }

        public override void VisitObjectId(PdfObjectId obj, StringBuilder context)
        {
            context.Append($"[blue]{obj.Number}[/]:[blue]{obj.Generation}[/]");
        }

        public override void VisitObjectStream(PdfObjectStream obj, StringBuilder context)
        {
            context.Append($"[silver][[ObjSt]][/] [green]{obj.ObjectCount}[/] " +
                           $"[silver]object(s),[/] [green]{obj.Length}[/] [silver]byte(s)[/]");
        }

        public override void VisitReal(PdfReal obj, StringBuilder context)
        {
            context.Append($"[silver][[Real]][/] [yellow]{obj.Value}[/]");
        }

        public override void VisitStream(PdfStream obj, StringBuilder context)
        {
            context.Append($"[silver][[Stream]][/] [yellow]{obj.Length}[/] [silver]byte(s)[/]");
        }

        public override void VisitString(PdfString obj, StringBuilder context)
        {
            context.Append($"[silver][[String]][/] [yellow]{obj.Value}[/] [silver]({obj.Encoding})[/]");
        }
    }
}