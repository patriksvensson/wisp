namespace Wisp.Tests;

public sealed partial class CosDocumentTests
{
    public sealed class Objects
    {
        [Fact]
        public void Should_Resolve_Indirect_Object_From_Object_Stream()
        {
            // Given
            var fixture = new CosDocumentFixture.Simple();
            var document = fixture.Document;

            // When
            var obj = document.Objects.Get(7, 0);

            // Then
            obj.ShouldNotBeNull();
            obj.Object.ShouldBeOfType<CosDictionary>().And(dict =>
            {
                dict.GetInteger(CosNames.Count).ShouldHaveValue(2);
                dict.GetObjectReference(CosNames.First).ShouldBe(8, 0);
                dict.GetObjectReference(CosNames.Last).ShouldBe(8, 0);
            });
        }
    }
}