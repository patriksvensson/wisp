namespace Wisp;

[PublicAPI]
public static class CosTrailerReader
{
    private static readonly byte[] _marker = [0x73, 0x74, 0x61, 0x72, 0x74, 0x78, 0x72, 0x65, 0x66];

    public static (CosXRefTable Table, CosTrailer Trailer) Read(CosParser parser)
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

            var table = default(CosXRefTable?);
            var trailer = default(CosDictionary?);

            while (xrefStart > 0)
            {
                parser.Seek(xrefStart.Value, SeekOrigin.Begin);
                var (readTable, readTrailer) = parser.PeekToken()?.Kind == CosTokenKind.XRef
                    ? CosXRefTableReader.Read(parser)
                    : CosXRefTableReader.ParseXRefStream(parser);

                table = table?.Merge(readTable) ?? readTable;
                trailer = readTrailer;

                var prev = trailer.GetInt64(CosNames.Prev);
                if (prev == null)
                {
                    break;
                }

                xrefStart = prev.Value;
            }

            return (
                Table: table ?? new CosXRefTable(),
                Trailer: new CosTrailer(trailer ?? new CosDictionary()));
        }
        finally
        {
            parser.Seek(previousPosition, SeekOrigin.Begin);
        }
    }

    private static long? FindXrefStart(CosParser parser)
    {
        // Back up 1024 bytes (or as much as the file allow)
        parser.Seek(-Math.Min(1024, parser.Length), SeekOrigin.End);

        var index = 0;
        var found = new List<long?>();
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
                var obj = parser.Parse();
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