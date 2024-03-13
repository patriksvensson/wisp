namespace Wisp;

[PublicAPI]
public sealed class CosWriterSettings
{
    public CosCompression Compression { get; set; } = CosCompression.Optimal;
    public bool LeaveStreamOpen { get; set; }

    public static CosWriterSettings WithoutCompression()
    {
        return new CosWriterSettings
        {
            Compression = CosCompression.None,
        };
    }
}