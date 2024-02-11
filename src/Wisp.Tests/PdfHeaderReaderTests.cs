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
        var result = PdfHeaderReader.ReadHeader(reader);

        // Then
        result.Version.ShouldBe(PdfVersion.Pdf1_6);
    }

    [Fact]
    public void Should_Return_Null_If_Not_A_Valid_Pdf_Header()
    {
        // Given
        var input = "Hello World";
        var reader = new BufferReader(input.ToStream());

        // When
        var result = Record.Exception(() => PdfHeaderReader.ReadHeader(reader));

        // Then
        result.ShouldBeOfType<InvalidOperationException>()
            .Message.ShouldBe("PDF file is missing header");
    }

    [Fact]
    public void Should_Return_Null_If_Version_Is_Unsupported()
    {
        // Given
        var input = "%PDF-1.8";
        var reader = new BufferReader(input.ToStream());

        // When
        var result = Record.Exception(() => PdfHeaderReader.ReadHeader(reader));

        // Then
        result.ShouldBeOfType<NotSupportedException>()
            .Message.ShouldBe("PDF version 1.8 is not supported");
    }
}