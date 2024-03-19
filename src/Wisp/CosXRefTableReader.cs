namespace Wisp;

[PublicAPI]
public static class CosXRefTableReader
{
    public static (CosXRefTable XRefTable, CosDictionary Trailer) Read(CosParser parser)
    {
        parser.ExpectToken(CosTokenKind.XRef);

        var table = new CosXRefTable();

        while (parser.CanRead)
        {
            if (!parser.CheckToken(CosTokenKind.Integer))
            {
                break;
            }

            var startId = parser.ExpectToken(CosTokenKind.Integer).ParseInt32();
            var count = parser.ExpectToken(CosTokenKind.Integer).ParseInt32();

            foreach (var id in Enumerable.Range(startId, count))
            {
                var position = parser.ExpectToken(CosTokenKind.Integer).ParseInt32();
                var generation = parser.ExpectToken(CosTokenKind.Integer).ParseInt32();
                var kind = parser.ReadToken().Kind;

                if (position == 0)
                {
                    // Microsoft Word adds empty rows in the xref table sometimes
                    // For now, just ignore since it doesn't point to an actual object
                    continue;
                }

                switch (kind)
                {
                    case CosTokenKind.XRefFree:
                        // We don't care of free objects
                        break;
                    case CosTokenKind.XRefIndirect:
                        table.Add(new CosIndirectXRef(
                            new CosObjectId(id, generation),
                            position));
                        break;
                    default:
                        throw new WispException("Unknown xref kind encountered");
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
                if (parser.Parse() is CosDictionary trailerDictionary)
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
        var primitive = parser.Parse();
        if (primitive is not CosObject obj || obj.Object is not CosStream stream)
        {
            throw new WispException("Expected COS stream");
        }

        var table = new CosXRefTable();

        var sizes = GetFieldSizes(stream);
        var ids = GetObjectIds(stream);

        foreach (var entry in ReadEntries(stream, sizes))
        {
            if (ids.Count == 0)
            {
                throw new WispException("Cannot read xref stream (no more index)");
            }

            // Get the next object ID.
            var id = ids.Dequeue();

            if (entry.First == 0)
            {
                // We don't care of free objects
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
                throw new WispException(
                    "Unknown xref stream object type");
            }
        }

        return (table, stream.Dictionary);
    }

    private static int[] GetFieldSizes(CosStream stream)
    {
        var sizes = stream.Dictionary.GetArray(CosNames.W);
        if (sizes == null)
        {
            throw new WispException("XRef Stream is missing /W array");
        }

        if (sizes.Count != 3)
        {
            throw new WispException($"Expected 3 items in /W array in XRef stream. Found {sizes.Count}");
        }

        var w = new int[3];
        w[0] = sizes.GetInt32At(0) ?? 1;
        w[1] = sizes.GetInt32At(1) ?? 0;
        w[2] = sizes.GetInt32At(2) ?? 0;

        if (w[0] < 0 || w[1] < 0 || w[2] < 0)
        {
            throw new WispException("/W array in XRef stream is invalid");
        }

        return w;
    }

    private static Queue<int> GetObjectIds(CosStream stream)
    {
        var size = stream.Dictionary.GetInt64(CosNames.Size);
        if (size == null)
        {
            throw new WispException(
                "Stream xref table did not have size");
        }

        var indexArray = stream.Dictionary.GetArray(CosNames.Index);
        if (indexArray == null)
        {
            indexArray = new CosArray
            {
                new CosInteger(0),
                new CosInteger(size),
            };
        }

        var indices = new List<int>();
        foreach (var item in indexArray)
        {
            if (item is not CosInteger arrayInteger)
            {
                throw new WispException(
                    "Encountered malformed index array (not an integer)");
            }

            indices.Add((int)arrayInteger.Value);
        }

        if (indices.Count % 2 != 0)
        {
            throw new WispException(
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

    private static IEnumerable<(int First, int Second, int Third)> ReadEntries(CosStream stream, int[] sizes)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (sizes.Length != 3)
        {
            throw new WispException("Invalid sizes");
        }

        var data = stream.GetUnfilteredData();
        if (data is null)
        {
            yield break;
        }

        using var reader = new MemoryStream(data);
        while (reader.Position < reader.Length)
        {
            var first = Unpack(reader, sizes[0]);
            var second = Unpack(reader, sizes[1]);
            var third = Unpack(reader, sizes[2]);

            yield return (first, second, third);
        }

        yield break;

        // Unpacks an integer using n bytes in the stream
        static int Unpack(Stream stream, int length)
        {
            var accumulated = 0;
            for (var index = 0; index < length; index++)
            {
                var offset = 8 * (length - index - 1);
                var value = stream.ReadByte();
                accumulated |= offset == 0 ? value : value << offset;
            }

            return accumulated;
        }
    }
}