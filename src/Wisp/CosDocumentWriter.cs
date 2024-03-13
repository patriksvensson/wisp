namespace Wisp;

public static class CosDocumentWriter
{
    public static void Write(CosWriter writer)
    {
        // We explicitly set 1.6 since we're currently saving
        // the xref table as a xref stream.
        writer.Write($"%PDF-1.6\n");
        writer.Write("%");
        writer.Write([128, 129, 130, 131]);
        writer.Write("\n");

        // Write objects
        var positions = new Dictionary<CosObjectId, long>(CosObjectIdComparer.Shared);
        foreach (var obj in writer.Document.Objects)
        {
            positions.Add(obj.Id, writer.Position);
            writer.Write(obj);
            writer.Write('\n');
        }

        // Write the xref stream
        var start = CosXRefTableWriter.Write(writer, positions);

        // Write the end of the file
        writer.Write('\n');
        writer.Write("startxref\n");
        writer.Write(start);
        writer.Write("\n%%EOF\n");
    }
}