using System.Text;

namespace Wisp.Tests;

public class CosDocumentWriterTests
{
    [Fact(Skip = "")]
    public void Lol()
    {
        var fixture = new CosDocumentFixture.XRefStream();
        fixture.Document.Info.Author = new CosString("Patrik Svensson");

        var path = "/Users/patrik/Desktop/out.pdf";
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        // Save the document
        var stream = File.OpenWrite(path);
        fixture.Document.Save(stream);
    }

    [Fact(Skip = "")]
    public void Empty()
    {
        var document = new CosDocument();
        document.Info.Author = new CosString("Patrik Svensson");

        var path = "/Users/patrik/Desktop/Empty_out.pdf";
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        // Save the document
        var stream = File.OpenWrite(path);
        document.Save(stream);
    }

    [Fact(Skip = "")]
    public void Blank()
    {
        var fixture = CosDocument.Open(File.OpenRead("/Users/patrik/Desktop/Blank.pdf"));
        fixture.Info.Author = new CosString("Patrik Svensson");

        var path = "/Users/patrik/Desktop/Blank_out.pdf";
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        var stream = File.OpenWrite(path);
        fixture.Save(stream, CosCompression.None);
    }

    [Fact(Skip = "")]
    public void Lol2()
    {
        var path = "/Users/patrik/Desktop/Blank_out.pdf";
        var doc = CosDocument.Open(File.OpenRead(path));
    }
}