namespace Wisp.Tests;

public sealed class PdfTrailerReaderTests
{
    [Fact]
    public void Should_Read_XRef_Table()
    {
        // Given
        var reader = new BufferReader(
            """
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
            0
            %%EOF
            """.ToStream());

        // When
        var (table, _) = PdfTrailerReader.ReadTrailer(reader);

        // Then
        table.ShouldNotBeNull();
        table.GetReference(new PdfObjectId(4, 0))
            .ShouldBeOfType<PdfIndirectXRef>()
            .Position.ShouldBe(331);
    }

    [Fact]
    public void Should_Read_XRef_Table_With_Subsections()
    {
        // Given
        var reader = new BufferReader(
            """
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
            0
            %%EOF
            """.ToStream());

        // When
        var (table, _) = PdfTrailerReader.ReadTrailer(reader);

        // Then
        table.ShouldNotBeNull();
        table.GetReference(new PdfObjectId(23, 2))
            .ShouldBeOfType<PdfIndirectXRef>()
            .Position.ShouldBe(25518);
    }

    [Fact]
    public void Should_Read_Trailer()
    {
        // Given
        var reader = new BufferReader(
            """
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
            0
            %%EOF
            """.ToStream());

        // When
        var (_, trailer) = PdfTrailerReader.ReadTrailer(reader);

        // Then
        trailer.ShouldNotBeNull();
        trailer.Size.ShouldHaveValue(5);
        trailer.Root.ShouldHaveNumber(1).ShouldHaveGeneration(0);
    }
}