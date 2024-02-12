namespace Wisp;

internal static class PdfDocumentReader
{
    public static PdfDocument Read(Stream stream)
    {
        var reader = new PdfReader(stream);

        // Read the header
        var header = PdfHeaderReader.ReadHeader(reader.Reader);

        // Read the trailer and xref table
        var (table, trailer) = PdfTrailerReader.ReadTrailer(reader.Reader);

        // Put together the PDF document
        return new PdfDocument(
            header,
            table ?? new PdfXRefTable(),
            trailer ?? new PdfTrailer(new PdfDictionary()),
            reader);
    }
}