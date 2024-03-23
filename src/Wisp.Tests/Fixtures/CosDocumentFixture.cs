using JetBrains.Annotations;

namespace Wisp.Testing.Fixtures;

public class CosDocumentFixture
{
    public CosDocument Document { get; }

    private CosDocumentFixture(string path, CosReaderSettings? settings)
    {
        var stream = EmbeddedResourceReader.GetStream(path);
        Document = CosDocument.Open(stream, settings);
    }

    public sealed class Simple : CosDocumentFixture
    {
        private const string Path = "Wisp.Tests/Data/Simple.pdf";

        public Simple(CosReaderSettings? settings = null)
            : base(Path, settings)
        {
        }

        public static CosDocumentFixture Create(CosReaderSettings? settings = null)
        {
            return new CosDocumentFixture(Path, settings);
        }
    }

    public sealed class XRefStream : CosDocumentFixture
    {
        [UsedImplicitly]
        public XRefStream()
            : base("Wisp.Tests/Data/XRefStream.pdf", null)
        {
        }
    }
}