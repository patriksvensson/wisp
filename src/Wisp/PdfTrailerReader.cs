namespace Wisp;

public static class PdfTrailerReader
{
    private static readonly byte[] _marker = new byte[]
    {
        0x73, 0x74, 0x61, 0x72, 0x74,
        0x78, 0x72, 0x65, 0x66,
    };

    public static (PdfXRefTable? Table, PdfTrailer? Trailer) ReadTrailer(IBufferReader reader)
    {
        var parser = new PdfObjectParser(reader);
        var previousPosition = reader.Position;

        try
        {
            // Find where the xref table start
            var xrefStart = FindXrefStart(parser);
            if (xrefStart == null)
            {
                throw new InvalidOperationException("Could not find xref start");
            }

            // Read the xref table
            reader.Seek(xrefStart.Value, SeekOrigin.Begin);
            var table = parser.ReadObject() as PdfXRefTable;

            // Now find the trailer
            var trailer = default(PdfTrailer);
            while (parser.Reader.CanRead)
            {
                var current = parser.Lexer.Read();
                if (current.Kind == PdfObjectTokenKind.Trailer)
                {
                    var trailerDictionary = parser.ReadObject() as PdfDictionary;
                    if (trailerDictionary != null)
                    {
                        trailer = new PdfTrailer(trailerDictionary);
                        break;
                    }
                }
            }

            return (table, trailer);
        }
        finally
        {
            reader.Seek(previousPosition, SeekOrigin.Begin);
        }
    }

    private static int? FindXrefStart(PdfObjectParser parser)
    {
        // Back up 1024 bytes (or as much as the file allow)
        parser.Reader.Seek(-Math.Min(1024, parser.Reader.Length), SeekOrigin.End);

        var index = 0;
        while (parser.Reader.CanRead)
        {
            var current = parser.Reader.ReadByte();
            if (current == _marker[index])
            {
                index++;
            }
            else
            {
                index = 0;
            }

            if (index == _marker.Length)
            {
                var obj = parser.ReadObject();
                if (obj is not PdfInteger integer)
                {
                    throw new InvalidOperationException("Expected startxref to be an integer");
                }

                return integer.Value;
            }
        }

        return null;
    }
}