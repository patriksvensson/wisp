namespace Wisp.Testing.Fixtures;

public abstract class TrailerFixture
{
    public CosXRefTable? XRefTable { get; }
    public CosDictionary? Trailer { get; }

    public TrailerFixture(string path)
    {
        var stream = EmbeddedResourceReader.GetStream(path);
        var parser = new CosParser(stream!);
        var (xrefTable, trailer) = CosTrailerReader.Read(parser);

        XRefTable = xrefTable;
        Trailer = trailer;
    }

    public sealed class Default : TrailerFixture
    {
        public Default()
            : base("Wisp.Tests/Data/XRefStream.pdf")
        {
        }
    }

    public sealed class Incremental : TrailerFixture
    {
        public Incremental()
            : base("Wisp.Tests/Data/XRefStreamIncremental.pdf")
        {
        }
    }
}