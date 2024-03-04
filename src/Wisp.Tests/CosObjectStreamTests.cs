using Wisp.Testing.Fixtures;

namespace Wisp.Tests;

public sealed class CosObjectStreamTests : IClassFixture<CosFixture.XRefStream>
{
    private readonly CosFixture.XRefStream _fixture;

    public CosObjectStreamTests(CosFixture.XRefStream fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    [Fact]
    public void Should_Extract_Indirect_Object_From_Stream()
    {
        var id = new CosObjectId(7, 0);

        // Get the xref of the object stream
        var objectStreamRef = _fixture.XRefTable!.GetXRef(id)
            .ShouldBeOfType<CosStreamXRef>();

        // Get the xref of the stream
        var streamRef = _fixture.XRefTable!.GetXRef(objectStreamRef.StreamId)
            .ShouldBeOfType<CosIndirectXRef>();

        // Parse out the object at the specified position
        _fixture.Parser.Seek(streamRef.Position, SeekOrigin.Begin);
        var streamObj = _fixture.Parser.ParseObject().ShouldBeOfType<CosObject>()
            .Object.ShouldBeOfType<CosObjectStream>();

        // Now get the object we're looking for within the stream
        var result = streamObj.GetObject(id)
            .Object.ShouldBeOfType<CosDictionary>();

        // Ensure that the contents of the object is correct
        result.GetRequired<CosInteger>(CosName.Known.Count).ShouldHaveValue(2);
        result.GetRequired<CosObjectId>(CosName.Known.First).ShouldBe(8, 0);
        result.GetRequired<CosObjectId>(CosName.Known.Last).ShouldBe(8, 0);
    }
}