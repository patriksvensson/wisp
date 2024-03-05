namespace Wisp.Testing.Fixtures;

public abstract class CosDocumentFixture
{
    public CosDocument Document { get; set; }

    protected CosDocumentFixture(string path)
    {
        var stream = EmbeddedResourceReader.GetStream(path);
        Document = CosDocument.Open(stream);
    }

    public sealed class XRefStream : CosDocumentFixture
    {
        public XRefStream()
            : base("Wisp.Tests/Data/XRefStream.pdf")
        {
        }
    }

    public sealed class XRefStreamIncremental : CosDocumentFixture
    {
        public XRefStreamIncremental()
            : base("Wisp.Tests/Data/XRefStreamIncremental.pdf")
        {
        }
    }
}