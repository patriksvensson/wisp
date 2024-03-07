namespace Wisp.Cos;

[PublicAPI]
public static class CosDocumentReader
{
    public static CosDocument Read(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

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

        // Get the document information
        var info = default(CosInfo);
        if (trailer.Info != null)
        {
            var infoObj = resolver.GetObject(trailer.Info);
            if (infoObj == null)
            {
                // TODO: We should remove the info object from the trailer
                throw new InvalidOperationException(
                    "Info object was specified but did not exist");
            }

            var infoDictionary = infoObj.Object as CosDictionary;
            if (infoDictionary == null)
            {
                throw new InvalidOperationException(
                    "Info object was expected to be a dictionary, but was not");
            }

            info = new CosInfo(trailer.Info, infoDictionary);
        }

        return new CosDocument(
            version, xRefTable, trailer,
            info, resolver);
    }
}