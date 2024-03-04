using System.IO.Compression;

namespace Wisp.Filters;

[PublicAPI]
public sealed class FlateFilter : Filter
{
    public override string Name { get; } = "FlateDecode";

    public override byte[] Decode(byte[] data, CosDictionary parameters)
    {
        // Run the deflate algorithm
        var bytes = Deflate(data);

        var predictor = parameters.GetInt32(CosName.Known.Predictor) ?? 1;
        if (predictor != 1)
        {
            // TODO: Support PNG encoding
            throw new NotSupportedException(
                "Additional decoding is required, but not yet supported");
        }

        return bytes;
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
}