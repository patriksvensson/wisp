namespace Wisp.Tests;

public sealed class PdfHeaderReaderTests
{
    [Fact]
    public void Should_Read_Version()
    {
        // Given
        var input = "%PDF-1.6";
        var reader = new BufferReader(input.ToStream());

        // When
        var result = PdfHeaderReader.TryReadVersion(reader, out var version);

        // Then
        result.ShouldBeTrue();
        version.ShouldBe(PdfVersion.Pdf1_6);
    }

    [Fact]
    public void Should_Return_Null_If_Not_A_Valid_Pdf_Header()
    {
        // Given
        var input = "Hello World";
        var reader = new BufferReader(input.ToStream());

        // When
        var result = PdfHeaderReader.TryReadVersion(reader, out var version);

        // Then
        result.ShouldBeFalse();
        version.ShouldBeNull();
    }

    [Fact]
    public void Should_Return_Null_If_Version_Is_Unsupported()
    {
        // Given
        var input = "%PDF-1.8";
        var reader = new BufferReader(input.ToStream());

        // When
        var result = PdfHeaderReader.TryReadVersion(reader, out var version);

        // Then
        result.ShouldBeFalse();
        version.ShouldBeNull();
    }
}