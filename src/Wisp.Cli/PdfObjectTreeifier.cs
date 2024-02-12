using Spectre.Console;

namespace Wisp.Cli;

public static class PdfObjectTreeifier
{
    public static Tree ToTree(string root, PdfObject obj)
    {
        var tree = new Tree(root);
        var visitor = new Visitor();
        obj.Accept(visitor, tree);
        return tree;
    }

    public sealed class Visitor : PdfObjectVisitor<IHasTreeNodes>
    {
        public override void VisitArray(PdfArray obj, IHasTreeNodes context)
        {
            var node = context.AddNode(PdfObjectMarkupifier.ToMarkup(obj));
            foreach (var item in obj)
            {
                item.Accept(this, node);
            }
        }

        public override void VisitBoolean(PdfBoolean obj, IHasTreeNodes context)
        {
            context.AddNode(PdfObjectMarkupifier.ToMarkup(obj));
        }

        public override void VisitDictionary(PdfDictionary obj, IHasTreeNodes context)
        {
            var root = context.AddNode(PdfObjectMarkupifier.ToMarkup(obj));
            foreach (var kvp in obj)
            {
                var key = PdfObjectMarkupifier.ToMarkup(kvp.Key);

                if (kvp.Value is PdfArray || kvp.Value is PdfDictionary)
                {
                    var node = root.AddNode(key);
                    kvp.Value.Accept(this, node);
                }
                else
                {
                    var value = PdfObjectMarkupifier.ToMarkup(kvp.Value);
                    root.AddNode($"{key} = {value}");
                }
            }
        }

        public override void VisitInteger(PdfInteger obj, IHasTreeNodes context)
        {
            context.AddNode(PdfObjectMarkupifier.ToMarkup(obj));
        }

        public override void VisitName(PdfName obj, IHasTreeNodes context)
        {
            context.AddNode(PdfObjectMarkupifier.ToMarkup(obj));
        }

        public override void VisitNull(PdfNull obj, IHasTreeNodes context)
        {
            context.AddNode(PdfObjectMarkupifier.ToMarkup(obj));
        }

        public override void VisitObjectDefinition(PdfObjectDefinition obj, IHasTreeNodes context)
        {
            obj.Object.Accept(this, context);
        }

        public override void VisitObjectId(PdfObjectId obj, IHasTreeNodes context)
        {
            context.AddNode(PdfObjectMarkupifier.ToMarkup(obj));
        }

        public override void VisitObjectStream(PdfObjectStream obj, IHasTreeNodes context)
        {
            var node = context.AddNode(PdfObjectMarkupifier.ToMarkup(obj));
            for (int index = 0; index < obj.ObjectCount; index++)
            {
                obj.GetObjectByIndex(index).Accept(this, node);
            }
        }

        public override void VisitReal(PdfReal obj, IHasTreeNodes context)
        {
            context.AddNode(PdfObjectMarkupifier.ToMarkup(obj));
        }

        public override void VisitStream(PdfStream obj, IHasTreeNodes context)
        {
            context.AddNode("Stream");
        }

        public override void VisitString(PdfString obj, IHasTreeNodes context)
        {
            context.AddNode(PdfObjectMarkupifier.ToMarkup(obj));
        }
    }
}