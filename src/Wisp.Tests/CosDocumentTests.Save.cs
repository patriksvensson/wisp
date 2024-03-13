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

    [Fact]
    public void Lol()
    {
    var document = CosDocument.Open(
        File.OpenRead(
            "C:/Users/Patrik/Desktop/Blank.pdf"));

    // Create a new object
    var obj = new CosObject(
        document.XRefTable.GetNextId(),
        new CosDictionary()
        {
            { new CosName("/Foo"), new CosInteger(32) },
            { new CosName("/Bar"), new CosString("Patrik") },
        });

    // Add the created object to the document
    document.Objects.Set(obj);

    // Get an object from the document and manipulate it
    // In this case we know that the object 25:0 exist, but this
    // will most certainly crash if you're running this as is.
    var other = document.Objects.Get(number: 1, generation: 0);
    ((CosDictionary)other.Object)[new CosName("Baz")] = new CosString("Hello");

    // Change the author of the document
    document.Info.Title = new CosString("Wisp test");
    document.Info.Author = new CosString("Patrik Svensson");

    // Save the document
    document.Save(
        File.OpenWrite("C:/Users/Patrik/Desktop/Blank__out.pdf"),
        CosCompression.Smallest);
    }
}