namespace Wisp;

internal static class PdfTrailerReader
{
    private static readonly byte[] _marker = new byte[] { 0x73, 0x74, 0x61, 0x72, 0x74, 0x78, 0x72, 0x65, 0x66, };

    public static (PdfXRefTable? Table, PdfTrailer? Trailer) ReadTrailer(PdfReader reader)
    {
        var previousPosition = reader.Position;

        try
        {
            // Find where the xref table start
            var xrefStart = FindXrefStart(reader);
            if (xrefStart == null)
            {
                throw new InvalidOperationException("Could not find xref start");
            }

            var table = new PdfXRefTable();
            var trailer = default(PdfTrailer);
            while (true)
            {
                var (readTable, readTrailer) = ReadXRefTableAndTrailer(reader, xrefStart);

                if (readTable != null)
                {
                    // Merge the two tables together
                    table.Merge(readTable);
                }

                if (readTrailer != null)
                {
                    trailer = readTrailer;

                    if (readTrailer.Prev != null)
                    {
                        xrefStart = readTrailer.Prev.Value;
                        continue;
                    }
                }

                break;
            }

            return (table, trailer);
        }
        finally
        {
            reader.Seek(previousPosition, SeekOrigin.Begin);
        }
    }

    private static (PdfXRefTable? Table, PdfTrailer? Trailer)
        ReadXRefTableAndTrailer(
            PdfReader reader,
            [DisallowNull] int? xrefStart)
    {
        // Read the xref table
        reader.Seek(xrefStart.Value, SeekOrigin.Begin);
        var table = reader.ReadObject() as PdfXRefTable;

        // Now find the trailer
        var trailer = default(PdfTrailer);
        while (reader.CanRead)
        {
            var current = reader.ReadToken();
            if (current.Kind == PdfObjectTokenKind.Trailer)
            {
                var trailerDictionary = reader.ReadObject() as PdfDictionary;
                if (trailerDictionary != null)
                {
                    trailer = new PdfTrailer(trailerDictionary);
                    break;
                }
            }
        }

        return (table, trailer);
    }

    private static int? FindXrefStart(PdfReader reader)
    {
        // Back up 1024 bytes (or as much as the file allow)
        reader.Seek(-Math.Min(1024, reader.Length), SeekOrigin.End);

        var index = 0;
        var found = new List<int?>();
        while (reader.CanRead)
        {
            var current = reader.ReadByte();
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
                var obj = reader.ReadObject();
                if (obj is not PdfInteger integer)
                {
                    throw new InvalidOperationException("Expected startxref to be an integer");
                }

                found.Add(integer.Value);
                index = 0;
            }
        }

        return found.LastOrDefault();
    }
}