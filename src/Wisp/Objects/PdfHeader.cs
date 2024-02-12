namespace Wisp;

public sealed class PdfHeader
{
    public PdfVersion Version { get; set; }

    public PdfHeader(PdfVersion version)
    {
        Version = version;
    }
}