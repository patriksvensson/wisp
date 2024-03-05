using Wisp.Testing.Fixtures;

namespace Wisp.Tests;

public sealed partial class CosDocumentTests
{
    [Fact]
    public void Should_Resolve_Indirect_Object_From_Object_Stream()
    {
        // Given
        var stream = EmbeddedResourceReader.GetStream("Wisp.Tests/Data/Simple.pdf");
        var document = CosDocument.Open(stream);

        // When
        var obj = document.Objects.GetById(7, 0);

        // Then
        obj.ShouldNotBeNull();
        obj.Object.ShouldBeOfType<CosDictionary>().And(dict =>
        {
            dict.GetInteger(CosNames.Count).ShouldHaveValue(2);
            dict.GetObjectId(CosNames.First).ShouldBe(8, 0);
            dict.GetObjectId(CosNames.Last).ShouldBe(8, 0);
        });
    }
}