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
    }
}