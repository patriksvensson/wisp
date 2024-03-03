using System.IO.Compression;

namespace Wisp.Internal;

internal sealed class DeflateFilter : Filter
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
            throw new NotSupportedException(
                "Additional decoding is required, but not yet supported");
        }

        return bytes;
    }

    private static byte[] Deflate(byte[] data)
    {
        var output = new MemoryStream();
        using (var input = new MemoryStream(data.Skip(2).ToArray())) // TODO: Not OK!
        using (var decoder = new DeflateStream(input, CompressionMode.Decompress))
        {
            decoder.CopyTo(output);
        }

        return output.ToArray();
    }
}