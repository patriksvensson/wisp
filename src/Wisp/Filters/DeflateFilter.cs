using System.IO.Compression;

namespace Wisp.Filters;

public sealed class DeflateFilter : Filter
{
    private readonly CosDictionary _parameters;

    public DeflateFilter(CosDictionary parameters)
    {
        _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    public override byte[] Decode(byte[] data)
    {
        // Run the deflate algorithm
        var bytes = Deflate(data);

        var predictor = _parameters.GetInt32(CosName.Known.Predictor) ?? 1;
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