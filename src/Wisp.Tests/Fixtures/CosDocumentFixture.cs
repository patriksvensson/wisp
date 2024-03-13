namespace Wisp.Testing.Fixtures;

public abstract class CosDocumentFixture
{
    public CosDocument Document { get; }

    private CosDocumentFixture(string path)
    {
        var stream = EmbeddedResourceReader.GetStream(path);
        Document = CosDocument.Open(stream);
    }

    public sealed class Simple : CosDocumentFixture
    {
        public Simple()
            : base("Wisp.Tests/Data/Simple.pdf")
        {
        }
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