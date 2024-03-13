namespace Wisp;

public static class CosDocumentWriter
{
    public static void Write(CosDocument document, CosWriter writer)
    {
        // We explicitly set 1.6 since we're currently saving
        // the xref table as a xref stream.
        writer.WriteLiteral("%PDF-1.6\n");
        writer.WriteLiteral("%");
        writer.WriteBytes([128, 129, 130, 131]);
        writer.WriteLiteral("\n");

        // Write objects
        var positions = new Dictionary<CosObjectId, long>(CosObjectIdComparer.Shared);
        foreach (var obj in document.Objects)
        {
            positions.Add(obj.Id, writer.Position);
            writer.Write(document, obj);
            writer.WriteByte('\n');
        }

        // Write the xref stream
        var start = CosXRefTableWriter.Write(document, writer, positions);

        // Write the end of the file
        writer.WriteByte('\n');
        writer.WriteLiteral("startxref\n");
        writer.WriteLiteral(start);
        writer.WriteLiteral("\n%%EOF\n");
    }
}