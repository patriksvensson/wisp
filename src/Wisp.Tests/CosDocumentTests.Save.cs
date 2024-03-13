namespace Wisp.Tests;

public sealed partial class CosDocumentTests
{
    [Theory]
    [InlineData(CosCompression.None)]
    [InlineData(CosCompression.Optimal)]
    [InlineData(CosCompression.Smallest)]
    [InlineData(CosCompression.Fastest)]
    public void Can_Read_Saved_File(CosCompression compression)
    {
        // Load the document
        var fixture = new CosDocumentFixture.Simple();
        var document = fixture.Document;

        // Change some things
        document.Info.Author = new CosString("Patrik Svensson");

        // Save the document to a stream
        var stream = new MemoryStream();
        document.Save(stream, compression, leaveOpen: true);

        // Reload the document
        stream.Seek(0, SeekOrigin.Begin);
        var newDocument = CosDocument.Open(stream);

        // Assert value
        newDocument.Info.Author.ShouldHaveValue("Patrik Svensson");
    }
}