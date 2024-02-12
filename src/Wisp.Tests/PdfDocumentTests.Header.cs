namespace Wisp.Tests;

public sealed partial class PdfDocumentTests
{
    public sealed class Header
    {
        [Fact]
        public void Should_Read_Version_From_Header()
        {
            // Given, When
            var document = PdfDocument.Read(
                """
                %PDF-1.6
                xref
                0 0
                trailer
                <<
                /Size 0
                >>
                startxref
                9
                %%EOF
                """.ToStream());

            // Then
            document.Header.Version.ShouldBe(PdfVersion.Pdf1_6);
        }

        [Fact]
        public void Should_Throw_If_Header_Is_Malformed()
        {
            // Given
            var input = "Hello World".ToStream();

            // When
            var result = Record.Exception(() => PdfDocument.Read(input));

            // Then
            result.ShouldBeOfType<InvalidOperationException>()
                .Message.ShouldBe("PDF file is missing header");
        }

        [Fact]
        public void Should_Throw_If_Pdf_Version_Is_Unsupported()
        {
            // Given
            var input = "%PDF-1.8".ToStream();

            // When
            var result = Record.Exception(() => PdfDocument.Read(input));

            // Then
            result.ShouldBeOfType<NotSupportedException>()
                .Message.ShouldBe("PDF version 1.8 is not supported");
        }
    }
}