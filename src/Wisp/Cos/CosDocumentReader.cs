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

        // Create the document
        return new CosDocument(version, xRefTable, trailer, resolver);
    }
}