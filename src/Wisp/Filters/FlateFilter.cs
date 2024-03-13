using System.IO.Compression;

namespace Wisp.Filters;

[PublicAPI]
public sealed class FlateFilter : Filter
{
    public override string Name { get; } = "FlateDecode";

    public override byte[] Decode(byte[] data, CosDictionary? parameters)
    {
        // Run the deflate algorithm
        var bytes = Deflate(data);

        var settings = GetPredictorSettings(parameters);
        if (settings.Predictor == 1)
        {
            return bytes;
        }

        if (settings.Predictor == 2)
        {
            throw new NotSupportedException("TIFF predictor not supported");
        }

        return PngDecoder.Decode(bytes, settings.Columns, settings.Colors, settings.BitsPerComponent);
    }

    public static byte[] Encode(byte[] data, CosCompression compression)
    {
        using (var original = new MemoryStream(data))
        using (var output = new MemoryStream())
        {
            // Write the flate header
            output.Write(new byte[] { 120, 156 });

            var level = compression switch
            {
                CosCompression.None => CompressionLevel.NoCompression,
                CosCompression.Fastest => CompressionLevel.Fastest,
                CosCompression.Optimal => CompressionLevel.Optimal,
                CosCompression.Smallest => CompressionLevel.SmallestSize,
                _ => throw new ArgumentOutOfRangeException(nameof(compression), compression, null),
            };

            using (var compressor = new DeflateStream(output, level))
            {
                original.CopyTo(compressor);
                compressor.Flush();
                return output.ToArray();
            }
        }
    }

    private static byte[] Deflate(byte[] data)
    {
        if (data.Length < 2)
        {
            throw new InvalidOperationException("Invalid flate stream");
        }

        var output = new MemoryStream();
        using (var input = new MemoryStream(data))
        {
            // The deflate stream does not support the
            // header, so just consume the first two bytes.
            input.ReadByte();
            input.ReadByte();

            using (var decoder = new DeflateStream(input, CompressionMode.Decompress))
            {
                decoder.CopyTo(output);
            }
        }

        return output.ToArray();
    }

    private static (int Predictor, int Columns, int Colors, int BitsPerComponent)
        GetPredictorSettings(CosDictionary? parameters)
    {
        var predictor = parameters?.GetInt32(CosNames.Predictor) ?? 1;
        var columns = parameters?.GetInt32(CosNames.Columns) ?? 1;
        var colors = parameters?.GetInt32(CosNames.Colors) ?? 1;
        var bits = parameters?.GetInt32(CosNames.BitsPerComponent) ?? 8;

        return (predictor, columns, colors, bits);
    }

    private static class PngDecoder
    {
        public static byte[] Decode(byte[] bytes, int columns, int colors, int bitsPerComponent)
        {
            var bytesPerRow = ((colors * columns * bitsPerComponent) + 7) / 8;

            var reader = new BinaryReader(new MemoryStream(bytes));
            var writer = new MemoryStream(bytes.Length);

            var previous = default(byte[]);

            while (true)
            {
                var filter = reader.Read();
                if (filter < 0)
                {
                    return writer.ToArray();
                }

                var current = new byte[bytesPerRow];
                ReadBytes(reader, current, bytesPerRow);

                if (filter == 0)
                {
                    // NONE
                }
                else if (filter == 1)
                {
                    // SUB
                    throw new NotSupportedException("Unsupported filter: PngSub");
                }
                else if (filter == 2)
                {
                    // UP
                    if (previous != null)
                    {
                        for (var i = 0; i < bytesPerRow; i++)
                        {
                            current[i] += previous[i];
                        }
                    }
                }
                else if (filter == 3)
                {
                    // AVERAGE
                    throw new NotSupportedException("Unsupported filter: PngAverage");
                }
                else if (filter == 4)
                {
                    // PAETH
                    throw new NotSupportedException("Unsupported filter: PngPaeth");
                }
                else if (filter == 5)
                {
                    // PAETH
                    throw new NotSupportedException("Unsupported filter: PngOptimum");
                }
                else
                {
                    // UNKNOWN
                    throw new NotSupportedException("Encountered unknown PNG filter during decoding");
                }

                // Write the current row to the stream
                writer.Write(current);

                // Swap streams
                previous = current;
            }
        }

        // TODO: Rewrite
        public static void ReadBytes(BinaryReader reader, byte[] buffer, int count)
        {
            if (count < 0)
            {
                throw new IndexOutOfRangeException();
            }

            var off = 0;
            var n = 0;
            while (n < count)
            {
                int read = reader.Read(buffer, off + n, count - n);
                if (read <= 0)
                {
                    throw new EndOfStreamException();
                }

                n += read;
            }
        }
    }
}