namespace Wisp.Tests;

public sealed partial class PdfDocumentTests
{
    public sealed class Trailer
    {
        [Fact]
        public void Should_Read_XRef_Table()
        {
            // Given
            var document = PdfDocument.Read(
                """
                %PDF-1.6
                xref
                0 6
                0000000000 65535 f
                0000000017 00000 n
                0000000081 00000 n
                0000000000 00007 f
                0000000331 00000 n
                0000000409 00000 n
                trailer
                <<
                /Root 1 0 R
                /Size 6
                >>
                startxref
                9
                %%EOF
                """.ToStream());

            // Then
            document.XRefTable.GetReference(new PdfObjectId(4, 0))
                .ShouldBeOfType<PdfIndirectXRef>()
                .Position.ShouldBe(331);
        }

        [Fact]
        public void Should_Read_XRef_Table_With_Prev()
        {
            // Given
            var document = PdfDocument.Read(
                """
                %PDF-1.6
                xref
                0 2
                0000000000 65535 f
                0000000017 00000 n
                trailer
                <<
                /Root 1 0 R
                /Size 2
                >>
                startxref
                0
                %%EOF
                xref
                10 2
                0000000000 65535 f
                0000000017 00000 n
                trailer
                <<
                /Root 1 0 R
                /Size 2
                /Prev 0
                >>
                startxref
                108
                %%EOF
                xref
                22 3
                0000000000 65535 f
                0000000054 00002 n
                0000000098 00000 n
                trailer
                <<
                /Root 1 0 R
                /Size 3
                /Prev 99
                >>
                startxref
                217
                %%EOF
                """.ToStream());

            // Then
            document.XRefTable.GetReference(new PdfObjectId(23, 2))
                .ShouldBeOfType<PdfIndirectXRef>()
                .Position.ShouldBe(54);
        }

        [Fact]
        public void Should_Read_XRef_Table_With_Subsections()
        {
            // Given
            var document = PdfDocument.Read(
                """
                %PDF-1.6
                xref
                0 1
                0000000000 65535 f
                3 1
                0000025325 00000 n
                23 2
                0000025518 00002 n
                0000025635 00000 n
                30 1
                0000025777 00000 n
                trailer
                <<
                /Root 1 0 R
                /Size 5
                >>
                startxref
                9
                %%EOF
                """.ToStream());

            // Then
            document.XRefTable.GetReference(new PdfObjectId(23, 2))
                .ShouldBeOfType<PdfIndirectXRef>()
                .Position.ShouldBe(25518);
        }

        [Fact]
        public void Should_Read_Trailer()
        {
            // Given
            var document = PdfDocument.Read(
                """
                %PDF-1.6
                xref
                0 6
                0000000000 65535 f
                0000000017 00000 n
                0000000081 00000 n
                0000000000 00007 f
                0000000331 00000 n
                0000000409 00000 n
                trailer
                <<
                /Root 1 0 R
                /Size 5
                >>
                startxref
                9
                %%EOF
                """.ToStream());

            // Then
            document.Trailer.ShouldNotBeNull();
            document.Trailer.Size.ShouldHaveValue(5);
            document.Trailer.Root.ShouldHaveNumber(1).ShouldHaveGeneration(0);
        }
    }
}