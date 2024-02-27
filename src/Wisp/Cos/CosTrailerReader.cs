namespace Wisp.Cos;

[PublicAPI]
public static class CosTrailerReader
{
    private static readonly byte[] _marker = [0x73, 0x74, 0x61, 0x72, 0x74, 0x78, 0x72, 0x65, 0x66];

    public static (CosXRefTable? Table, CosDictionary? Trailer) Read(CosParser parser)
    {
        var previousPosition = parser.Position;

        try
        {
            // Find where the xref table start
            var xrefStart = FindXrefStart(parser);
            if (xrefStart == null)
            {
                throw new InvalidOperationException("Could not find xref start");
            }

            var table = new CosXRefTable();
            var trailer = default(CosDictionary);
            while (true)
            {
                var (readTable, readTrailer) = ReadXRefTableAndTrailer(parser, xrefStart);
                if (readTable != null)
                {
                    table = table.Merge(readTable);
                }

                if (readTrailer != null)
                {
                    trailer = readTrailer;

                    var prev = trailer.GetOptionalValue<CosInteger>(CosName.Known.Prev);
                    if (prev != null)
                    {
                        xrefStart = prev.Value;
                        continue;
                    }
                }

                break;
            }

            return (table, trailer);
        }
        finally
        {
            parser.Seek(previousPosition, SeekOrigin.Begin);
        }
    }

    private static (CosXRefTable? Table, CosDictionary? Trailer) ReadXRefTableAndTrailer(
        CosParser parser, [DisallowNull] int? xrefStart)
    {
        // Read the xref table
        parser.Seek(xrefStart.Value, SeekOrigin.Begin);
        var table = CosXRefTableParser.Parse(parser);

        // Now find the trailer
        var trailer = default(CosDictionary);
        while (parser.CanRead)
        {
            var current = parser.ReadToken();
            if (current.Kind == CosTokenKind.Trailer)
            {
                if (parser.ParseObject() is CosDictionary trailerDictionary)
                {
                    trailer = trailerDictionary;
                    break;
                }
            }
        }

        return (table, trailer);
    }

    private static int? FindXrefStart(CosParser parser)
    {
        // Back up 1024 bytes (or as much as the file allow)
        parser.Seek(-Math.Min(1024, parser.Length), SeekOrigin.End);

        var index = 0;
        var found = new List<int?>();
        while (parser.CanRead)
        {
            var current = parser.ReadByte();
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
                var obj = parser.ParseObject();
                if (obj is not CosInteger integer)
                {
                    throw new InvalidOperationException("Expected 'startxref' to be an integer");
                }

                found.Add(integer.Value);
                index = 0;
            }
        }

        return found.LastOrDefault();
    }
}