namespace Wisp;

internal sealed class PdfHeaderReader
{
    public static PdfHeader ReadHeader(IByteReader reader)
    {
        var previousPosition = reader.Position;

        try
        {
            reader.Seek(0, SeekOrigin.Begin);
            var buffer = reader.ReadBytes(8);

            var text = Encoding.UTF8.GetString(buffer);
            var index = text.IndexOf("%PDF-", StringComparison.Ordinal);
            if (index == -1)
            {
                throw new InvalidOperationException("PDF file is missing header");
            }

            var versionNumber = text.Substring(index + 5, 3);
            return new PdfHeader(versionNumber switch
            {
                "1.0" => PdfVersion.Pdf1_0,
                "1.1" => PdfVersion.Pdf1_1,
                "1.2" => PdfVersion.Pdf1_2,
                "1.3" => PdfVersion.Pdf1_3,
                "1.4" => PdfVersion.Pdf1_4,
                "1.5" => PdfVersion.Pdf1_5,
                "1.6" => PdfVersion.Pdf1_6,
                "1.7" => PdfVersion.Pdf1_7,
                _ => throw new NotSupportedException($"PDF version {versionNumber} is not supported"),
            });
        }
        finally
        {
            reader.Seek(previousPosition, SeekOrigin.Begin);
        }
    }
}