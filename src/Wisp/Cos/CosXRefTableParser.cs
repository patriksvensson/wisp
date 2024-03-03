namespace Wisp.Cos;

[PublicAPI]
public static class CosXRefTableParser
{
    public static (CosXRefTable XRefTable, CosDictionary Trailer) Parse(CosParser parser)
    {
        parser.Lexer.Expect(CosTokenKind.XRef);

        var table = new CosXRefTable();

        while (parser.Lexer.Reader.CanRead)
        {
            if (!parser.Lexer.Check(CosTokenKind.Integer))
            {
                break;
            }

            var startId = parser.Lexer.Expect(CosTokenKind.Integer).ParseInteger();
            var count = parser.Lexer.Expect(CosTokenKind.Integer).ParseInteger();

            foreach (var id in Enumerable.Range(startId, count))
            {
                var position = parser.Lexer.Expect(CosTokenKind.Integer).ParseInteger();
                var generation = parser.Lexer.Expect(CosTokenKind.Integer).ParseInteger();

                switch (parser.Lexer.Read().Kind)
                {
                    case CosTokenKind.XRefFree:
                        table.Add(new CosFreeXRef(new CosObjectId(id, generation)));
                        break;
                    case CosTokenKind.XRefIndirect:
                        table.Add(new CosIndirectXRef(
                            new CosObjectId(id, generation),
                            position));
                        break;
                    default:
                        throw new InvalidOperationException("Unknown xref kind encountered");
                }
            }
        }

        // Now find the trailer
        var trailer = new CosDictionary();
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

    public static (CosXRefTable XRefTable, CosDictionary Trailer) ParseXRefStream(CosParser parser)
    {
        var start = parser.Position;
        var primitive = parser.ParseObject();
        if (primitive is not CosObject obj || obj.Object is not CosStream stream)
        {
            throw new InvalidOperationException("Expected COS stream");
        }

        var table = new CosXRefTable();

        // Add ourselves (the xref stream object) to the table
        table.Add(new CosIndirectXRef(obj.Id, start));

        var sizes = GetFieldSizes(stream);
        var ids = GetObjectIds(stream);

        foreach (var entry in ReadEntries(stream, sizes))
        {
            if (ids.Count == 0)
            {
                throw new InvalidOperationException("Cannot read xref stream (no more index)");
            }

            // Get the next object ID.
            var id = ids.Dequeue();

            if (entry.First == 0)
            {
                // Free object
                var generation = entry.Third;
                table.Add(new CosFreeXRef(
                    new CosObjectId(entry.Second, generation)));
            }
            else if (entry.First == 1)
            {
                // Indirect object
                var generation = entry.Third;
                var offset = entry.Second;
                table.Add(new CosIndirectXRef(
                    new CosObjectId(id, generation),
                    offset));
            }
            else if (entry.First == 2)
            {
                // Indirect object in stream
                var streamId = entry.Second;
                var streamIndex = entry.Third;
                table.Add(new CosStreamXRef(
                    new CosObjectId(id, 0),
                    new CosObjectId(streamId, 0),
                    streamIndex));
            }
            else
            {
                // No idea what this is
                // Corrupted data?
                throw new InvalidOperationException(
                    "Unknown xref stream object type");
            }
        }

        return (table, stream.Metadata);
    }

    private static int[] GetFieldSizes(CosStream stream)
    {
        var wArray = stream.Metadata.GetArray(CosName.Known.W);
        if (wArray == null)
        {
            throw new InvalidOperationException("XRef Stream is missing /W array");
        }

        if (wArray.Count != 3)
        {
            throw new InvalidOperationException($"Expected 3 items in /W array in XRef stream. Found {wArray.Count}");
        }

        var w = new int[3];
        w[0] = wArray.GetIntegerValue(0, 1);
        w[1] = wArray.GetIntegerValue(1, 0);
        w[2] = wArray.GetIntegerValue(2, 0);

        if (w[0] < 0 || w[1] < 0 || w[2] < 0)
        {
            throw new IOException("/W array in XRef stream is invalid");
        }

        return w;
    }

    private static Queue<int> GetObjectIds(CosStream stream)
    {
        var size = stream.Metadata.GetInt32(CosName.Known.Size);
        if (size == null)
        {
            throw new InvalidOperationException(
                "Stream xref table did not have size");
        }

        var indexArray = stream.Metadata.GetArray(CosName.Known.Index);
        if (indexArray == null)
        {
            indexArray = new CosArray
            {
                new CosInteger(0),
                new CosInteger(size.Value),
            };
        }

        var indices = new List<int>();
        foreach (var item in indexArray)
        {
            if (item is not CosInteger arrayInteger)
            {
                throw new InvalidOperationException(
                    "Encountered malformed index array (not an integer)");
            }

            indices.Add((int)arrayInteger.Value);
        }

        if (indices.Count % 2 != 0)
        {
            throw new InvalidOperationException(
                "Encountered malformed index array (unbalanced)");
        }

        var result = new List<int>();
        for (var i = 0; i < indices.Count; i += 2)
        {
            var start = indices[i];
            var count = indices[i + 1];

            result.AddRange(Enumerable.Range(start, count));
        }

        return new Queue<int>(result);
    }

    private static List<(int First, int Second, int Third)> ReadEntries(CosStream stream, int[] sizes)
    {
        static int Unpack(BinaryReader reader, int length)
        {
            var accumulated = 0;
            for (var index = 0; index < length; index++)
            {
                var offset = 8 * (length - index - 1);
                var value = reader.ReadByte();
                accumulated |= offset == 0 ? value : value << offset;
            }

            return accumulated;
        }

        ArgumentNullException.ThrowIfNull(stream);

        if (sizes.Length != 3)
        {
            throw new InvalidOperationException("Invalid sizes");
        }

        var data = stream.GetData();
        if (data is null)
        {
            return [];
        }

        using (var reader = new BinaryReader(new MemoryStream(data), Encoding.ASCII))
        {
            var result = new List<(int First, int Second, int Third)>();

            while (reader.PeekChar() != -1)
            {
                var first = Unpack(reader, sizes[0]);
                var second = Unpack(reader, sizes[1]);
                var third = Unpack(reader, sizes[2]);

                result.Add((first, second, third));
            }

            return result;
        }
    }
}