namespace Wisp.Filters;

[PublicAPI]
public sealed class CryptFilter : Filter
{
    public override string Name { get; } = "Crypt";

    public override byte[] Decode(byte[] data, CosDictionary? parameters)
    {
        var name = parameters?.GetName(CosNames.Name);
        if (name == null || name.Value.Equals("Identity", StringComparison.OrdinalIgnoreCase))
        {
            // Identity crypt filter just return the data as-is
            // See 7.6.5 in the PDF specification for more information
            return data;
        }

        throw new WispException(
            $"Unsupported crypt filter: {name.Value}");
    }
}