namespace Wisp;

[PublicAPI]
public static class CosDocumentReader
{
    public static CosDocument Read(Stream stream, CosReaderSettings? settings = null)
    {
        ArgumentNullException.ThrowIfNull(stream);

        settings ??= new CosReaderSettings();

        // Create the parser from the stream.
        // We will keep this parser around for the remainder of the created document.
        // The parser is responsible for reading objects from disk (or memory).
        var parser = new CosParser(stream);

        // Read the header
        var version = CosHeaderReader.Read(parser);

        // Read the xref table and trailer
        var (xRefTable, trailer) = CosTrailerReader.Read(parser);

        // Create the object resolver
        var resolver = new CosObjectResolver(parser, xRefTable);
        var objects = new CosObjectCache(xRefTable, resolver);

        // Unpack objects?
        if (settings.UnpackObjectStreams)
        {
            // Get all object streams
            var streams = xRefTable
                .OfType<CosStreamXRef>()
                .GroupBy(x => x.StreamId)
                .ToArray();

            foreach (var group in streams)
            {
                var cosObject = objects.Get(group.Key, CosResolveFlags.NoCache);
                if (cosObject?.Object is CosObjectStream objectStream)
                {
                    var objectIds = objectStream.GetObjectIds();
                    foreach (var objNumber in objectIds)
                    {
                        var objId = new CosObjectId(objNumber, 0);
                        var obj = objectStream.GetObject(objects, objId);
                        if (obj != null)
                        {
                            // Add the object to the cache
                            objects.Set(obj);

                            // Remove the object from the xref table
                            // and re-add it as an indirect object
                            xRefTable.Remove(obj.Id);
                            xRefTable.Add(new CosIndirectXRef(obj.Id));
                        }
                    }

                    // Remove the object stream itself from the xref table
                    // This will make sure that the object isn't written back.
                    xRefTable.Remove(cosObject.Id);
                }
            }
        }

        // Get the document information
        var info = default(CosInfo);
        if (trailer.Info != null)
        {
            var infoObj = objects.Get(trailer.Info);
            if (infoObj == null)
            {
                // TODO: We should remove the info object from the trailer
                throw new WispException(
                    "Info object was specified but did not exist");
            }

            if (infoObj.Object is not CosDictionary)
            {
                throw new WispException(
                    "Info object was expected to be a dictionary, but was not");
            }

            info = new CosInfo(infoObj);
        }

        return new CosDocument(
            version, xRefTable, trailer,
            info, objects, resolver);
    }
}