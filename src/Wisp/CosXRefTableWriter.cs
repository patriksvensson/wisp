namespace Wisp;

internal static class CosXRefTableWriter
{
    public static long Write(
        CosDocument document,
        CosWriter writer,
        CosXRefTable xRefTable,
        Dictionary<CosObjectId, long> positions)
    {
        var ids = xRefTable.Select(x => x.Id).Order().ToArray();
        var objectId = ids.Max(x => x.Number) + 1;

        var triplets = GetEntries(xRefTable, ids, positions);
        var sizes = triplets.GetSizes();
        var encoded = triplets.Encode(sizes);

        var dictionary = new CosDictionary(document.Trailer)
        {
            [CosNames.W] = sizes.ToCosArray(),
            [CosNames.Type] = new CosName("XRef"),
            [CosNames.Size] = new CosInteger(objectId + 1),
            [CosNames.Index] = triplets.GetIndexArray(),
            [CosNames.Length] = null,
            [CosNames.Filter] = null,
            [CosNames.Prev] = null,
        };

        // Create the object
        var stream = new CosStream(dictionary, encoded);
        var obj = new CosObject(new CosObjectId(objectId, 0), stream);

        // Write the stream
        var start = writer.Position;
        writer.Write(document, obj);

        return start;
    }

    private static Entries GetEntries(
        CosXRefTable xRefTable,
        CosObjectId[] ids,
        Dictionary<CosObjectId, long> positions)
    {
        var encoded = new List<CosXRef>();
        foreach (var id in ids)
        {
            var xref = xRefTable.GetXRef(id);
            if (xref == null)
            {
                throw new InvalidOperationException(
                    $"Could not find xref for object {id}");
            }

            xref = xref.CreateCopy();
            if (xref is CosIndirectXRef indirect)
            {
                // Update position
                indirect.Position = positions[xref.Id];
            }

            encoded.Add(xref);
        }

        var result = new List<Entry>
        {
            new Entry(Id: 0, 0, 0, 255),
        };

        foreach (var xref in encoded)
        {
            if (xref is CosIndirectXRef indirect)
            {
                Debug.Assert(indirect.Position != null, "Position should no able to be null for indirect object");
                result.Add(new Entry(xref.Id.Number, 1, indirect.Position.Value, xref.Id.Generation));
            }
            else if (xref is CosStreamXRef stream)
            {
                result.Add(new Entry(xref.Id.Number, 2, stream.StreamId.Number, stream.Index));
            }
            else
            {
                throw new InvalidOperationException("Unknown COS xref kind");
            }
        }

        return new Entries(result);
    }

    private sealed record Entry(int Id, long First, long Second, long Third);

    private sealed record EntrySize(int First, int Second, int Third)
    {
        public CosArray ToCosArray()
        {
            return new CosArray(new[] { new CosInteger(First), new CosInteger(Second), new CosInteger(Third), });
        }
    }

    private sealed class Entries(List<Entry> items)
    {
        public byte[] Encode(EntrySize sizes)
        {
            static void Write(MemoryStream stream, long value, int length)
            {
                var bytes = new byte[length];
                for (var offset = 0; offset < length; offset++)
                {
                    bytes[length - offset - 1] = (byte)(value & 0xFF);
                    value >>= 8; // Shift the value right by 8 bits to get the next byte
                }

                stream.Write(bytes);
            }

            var stream = new MemoryStream();
            foreach (var triplet in items)
            {
                Write(stream, triplet.First, sizes.First);
                Write(stream, triplet.Second, sizes.Second);
                Write(stream, triplet.Third, sizes.Third);
            }

            return stream.ToArray();
        }

        public CosArray GetIndexArray()
        {
            var result = new CosArray();
            foreach (var group in items.GroupConsecutive(i => i.Id))
            {
                result.Add(new CosInteger(group[0].Id));
                result.Add(new CosInteger(group.Length));
            }

            return result;
        }

        public EntrySize GetSizes()
        {
            static int GetBytesNeeded(long value)
            {
                var bytes = ((int)Math.Log(value, 2)) / 8;
                return bytes + 1;
            }

            static void UpdateBestFit(int value, ref int? best)
            {
                if (best == null)
                {
                    best = value;
                }
                else if (value > best)
                {
                    best = value;
                }
            }

            var bestFirst = default(int?);
            var bestSecond = default(int?);
            var bestThird = default(int?);

            foreach (var entry in items)
            {
                var first = GetBytesNeeded(entry.First);
                UpdateBestFit(first, ref bestFirst);

                var second = GetBytesNeeded(entry.Second);
                UpdateBestFit(second, ref bestSecond);

                var third = GetBytesNeeded(entry.Third);
                UpdateBestFit(third, ref bestThird);
            }

            return new EntrySize(bestFirst ?? 1, bestSecond ?? 0, bestThird ?? 0);
        }
    }
}