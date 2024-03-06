namespace Wisp.Tests;

public sealed partial class CosDocumentTests
{
    public sealed class Trailer : IClassFixture<CosDocumentFixture.XRefStream>
    {
        private readonly CosDocumentFixture.XRefStream _fixture;

        public Trailer(CosDocumentFixture.XRefStream fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public void Should_Read_Trailer_Properties_Correctly()
        {
            // Given, When
            var obj = _fixture.Document.Trailer;

            // Then
            obj.ShouldNotBeNull();
            obj.Root.ShouldBe(25, 0);
            obj.Info.ShouldBe(23, 0);
            obj.Prev.ShouldBeNull();
            obj.Encrypt.ShouldBeNull();
            obj.Id.ShouldNotBeNull()
                .And(id =>
                {
                    id[0].ShouldBeOfType<CosHexString>()
                        .ShouldHaveHexValue("A3112B77EA7D4BA78E81FE43166E2E51");

                    id[1].ShouldBeOfType<CosHexString>()
                        .ShouldHaveHexValue("617C8DB4AFBB4F57B2E6DDE44A9716BB");
                });
        }
    }
}