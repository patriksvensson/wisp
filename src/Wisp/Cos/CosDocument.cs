namespace Wisp.Cos;

[PublicAPI]
public sealed class CosDocument
{
    public CosDictionary Trailer { get; set; }

    public CosDocument()
    {
        Trailer = new CosDictionary();
    }
}