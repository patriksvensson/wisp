namespace Wisp.Testing.Fixtures;

public class CosFixture
{
    public CosParser Parser { get; }
    public CosXRefTable? XRefTable { get; }
    public CosDictionary? Trailer { get; }

    public CosFixture(string path)
    {
        var stream = EmbeddedResourceReader.GetStream(path);
        var parser = new CosParser(stream!);
        var (xrefTable, trailer) = CosTrailerReader.Read(parser);

        Parser = parser;
        XRefTable = xrefTable;
        Trailer = trailer;
    }

    public sealed class XRefStream : CosFixture
    {
        public XRefStream()
            : base("Wisp.Tests/Data/XRefStream.pdf")
        {
        }
    }
}