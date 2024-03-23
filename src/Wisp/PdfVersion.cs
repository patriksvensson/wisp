namespace Wisp;

[PublicAPI]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum PdfVersion
{
    Pdf1_0,
    Pdf1_1,
    Pdf1_2,
    Pdf1_3,
    Pdf1_4,
    Pdf1_5,
    Pdf1_6,
    Pdf1_7,
}

[PublicAPI]
public static class PdfVersionExtension
{
    public static string ToVersionString(this PdfVersion version)
    {
        return version switch
        {
            PdfVersion.Pdf1_0 => "1.0",
            PdfVersion.Pdf1_1 => "1.1",
            PdfVersion.Pdf1_2 => "1.2",
            PdfVersion.Pdf1_3 => "1.3",
            PdfVersion.Pdf1_4 => "1.4",
            PdfVersion.Pdf1_5 => "1.5",
            PdfVersion.Pdf1_6 => "1.6",
            PdfVersion.Pdf1_7 => "1.7",
            _ => throw new InvalidOperationException("Unsupported version"),
        };
    }
}