namespace Wisp;

public sealed class PdfHeader
{
    public PdfVersion Version { get; }

    public PdfHeader(PdfVersion version)
    {
        Version = version;
    }
}