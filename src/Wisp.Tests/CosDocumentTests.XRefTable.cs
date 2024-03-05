namespace Wisp.Tests;

public sealed partial class CosDocumentTests
{
    public sealed class XRefTable : IClassFixture<CosDocumentFixture.XRefStream>
    {
        private readonly CosDocumentFixture.XRefStream _fixture;

        public XRefTable(CosDocumentFixture.XRefStream fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public void Should_Contain_Entry_To_Indirect_Reference()
        {
            // Given, When
            var table = _fixture.Document.XRefTable;

            // Then
            table.ShouldNotBeNull();
            table.GetXRef(new CosObjectId(25, 0))
                .ShouldBeOfType<CosIndirectXRef>()
                .And(xref => xref.Position.ShouldBe(3419));
        }

        [Fact]
        public void Should_Contain_Entry_To_Stream_Reference()
        {
            // Given, When
            var table = _fixture.Document.XRefTable;

            // Then
            table.ShouldNotBeNull();
            table.GetXRef(new CosObjectId(10, 0))
                .ShouldBeOfType<CosStreamXRef>()
                .And(xref => xref.StreamId.ShouldHaveNumber(38))
                .And(xref => xref.Index.ShouldBe(3));
        }

        [Fact]
        public void Should_Contain_XRef_Stream_To_Xref_Table()
        {
            // Given, When
            var table = _fixture.Document.XRefTable;

            // Then
            table.ShouldNotBeNull();
            table.GetXRef(new CosObjectId(39, 0))
                .ShouldBeOfType<CosIndirectXRef>()
                .And(xref => xref.Position.ShouldBe(30198));
        }

        public sealed class Incremental : IClassFixture<CosDocumentFixture.XRefStreamIncremental>
        {
            private readonly CosDocumentFixture.XRefStreamIncremental _fixture;

            public Incremental(CosDocumentFixture.XRefStreamIncremental fixture)
            {
                _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
            }

            [Fact]
            public void Should_Contain_All_XRef_Streams()
            {
                // Given, When
                var stream = EmbeddedResourceReader.GetStream("Wisp.Tests/Data/XRefStreamIncremental.pdf");
                var document = CosDocument.Open(stream);

                // Then
                document.XRefTable.GetXRef(new CosObjectId(39, 0))
                    .ShouldBeOfType<CosIndirectXRef>()
                    .And(xref => xref.Position.ShouldBe(30198));

                document.XRefTable.GetXRef(new CosObjectId(40, 0))
                    .ShouldBeOfType<CosIndirectXRef>()
                    .And(xref => xref.Position.ShouldBe(30552));
            }
        }
    }
}