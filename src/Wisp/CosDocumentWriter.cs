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

        // Create a copy of the xref table since
        // we might have to manipulate it a bit
        var xrefTable = document.XRefTable.Clone();

        // Write objects
        var positions = new Dictionary<CosObjectId, long>(CosObjectIdComparer.Shared);
        foreach (var obj in document.Objects)
        {
            if (writer.Settings.UnpackObjectStreams &&
                obj.Object is CosObjectStream objectStream)
            {
                // Unpack all objects in the stream
                foreach (var id in objectStream.GetObjectIds())
                {
                    // Get the object from the stream
                    var objectStreamObjId = new CosObjectId(id, 0);
                    var objectStreamObj = objectStream.GetObject(
                        document.Objects, objectStreamObjId);

                    // Remove the item from the xref table,
                    // and add an indirect xref to the table
                    xrefTable.Remove(objectStreamObjId);
                    xrefTable.Add(new CosIndirectXRef(objectStreamObjId));

                    // Write the object
                    positions.Add(objectStreamObj.Id, writer.Position);
                    writer.Write(document, objectStreamObj);
                    writer.WriteByte('\n');
                }

                // Remove the main object from the xref table
                xrefTable.Remove(obj.Id);
            }
            else
            {
                // Write the object
                positions.Add(obj.Id, writer.Position);
                writer.Write(document, obj);
                writer.WriteByte('\n');
            }
        }

        // Write the xref stream
        var start = CosXRefTableWriter.Write(
            document, writer, xrefTable, positions);

        // Write the end of the file
        writer.WriteByte('\n');
        writer.WriteLiteral("startxref\n");
        writer.WriteLiteral(start);
        writer.WriteLiteral("\n%%EOF\n");
    }
}