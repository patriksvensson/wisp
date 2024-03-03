using Wisp.Testing.Fixtures;

namespace Wisp.Tests;

public sealed class TrailerTests
{
    public sealed class XRefStream : IClassFixture<TrailerFixture.Default>
    {
        private readonly TrailerFixture.Default _fixture;

        public XRefStream(TrailerFixture.Default fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public void Should_Add_Entry_To_Indirect_Reference()
        {
            // Given, When
            var table = _fixture.XRefTable;

            // Then
            table.ShouldNotBeNull();
            table.GetXRef(new CosObjectId(25, 0))
                .ShouldBeOfType<CosIndirectXRef>()
                .And(xref => xref.Position.ShouldBe(3419));
        }

        [Fact]
        public void Should_Add_Entry_To_Stream_Reference()
        {
            // Given, When
            var table = _fixture.XRefTable;

            // Then
            table.ShouldNotBeNull();
            table.GetXRef(new CosObjectId(10, 0))
                .ShouldBeOfType<CosStreamXRef>()
                .And(xref => xref.StreamId.ShouldHaveNumber(38))
                .And(xref => xref.Index.ShouldBe(3));
        }

        [Fact]
        public void Should_Add_XRef_Stream_To_Xref_Table()
        {
            // Given, When
            var table = _fixture.XRefTable;

            // Then
            table.ShouldNotBeNull();
            table.GetXRef(new CosObjectId(39, 0))
                .ShouldBeOfType<CosIndirectXRef>()
                .And(xref => xref.Position.ShouldBe(30198));
        }
    }

    public sealed class XRefStreamIncremental : IClassFixture<TrailerFixture.Incremental>
    {
        private readonly TrailerFixture.Incremental _fixture;

        public XRefStreamIncremental(TrailerFixture.Incremental fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public void Should_Add_All_XRef_Streams_To_Xref_Table()
        {
            // Given, When
            var table = _fixture.XRefTable;

            // Then
            table.ShouldNotBeNull();

            table.GetXRef(new CosObjectId(39, 0))
                .ShouldBeOfType<CosIndirectXRef>()
                .And(xref => xref.Position.ShouldBe(30198));

            table.GetXRef(new CosObjectId(40, 0))
                .ShouldBeOfType<CosIndirectXRef>()
                .And(xref => xref.Position.ShouldBe(30552));
        }
    }
}