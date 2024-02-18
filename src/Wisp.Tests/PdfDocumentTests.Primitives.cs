namespace Wisp.Tests;

public sealed partial class PdfDocumentTests
{
    public sealed class Fixture : IDisposable
    {
        public PdfDocument Document { get; }

        public Fixture()
        {
            Document = PdfDocument.Read(
                EmbeddedResourceReader.GetStream(
                    "Wisp.Tests/Data/Primitives.pdf")
                ?? throw new InvalidOperationException("Could not get PDF stream"));
        }

        public void Dispose()
        {
            Document.Dispose();
        }
    }

    public sealed class Primitives : IClassFixture<Fixture>
    {
        private readonly Fixture _fixture;

        public Primitives(Fixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Theory]
        [InlineData("1:0", true)]
        [InlineData("2:0", false)]
        public void Should_Parse_Boolean(string id, bool expected)
        {
            // Given, When
            var result = _fixture.Document.TryReadObject(
                PdfObjectId.Parse(id), out var obj);

            // Then
            result.ShouldBeTrue();
            obj.ShouldBeOfType<PdfObjectDefinition>()
                .Object.ShouldBeOfType<PdfBoolean>()
                .ShouldHaveValue(expected);
        }

        [Theory]
        [InlineData("3:0", 12)]
        [InlineData("4:0", -12)]
        [InlineData("5:0", 12)]
        public void Should_Parse_Integer(string id, int expected)
        {
            // Given, When
            var result = _fixture.Document.TryReadObject(
                PdfObjectId.Parse(id), out var obj);

            // Then
            result.ShouldBeTrue();
            obj.ShouldBeOfType<PdfObjectDefinition>()
                .Object.ShouldBeOfType<PdfInteger>()
                .ShouldHaveValue(expected);
        }

        [Theory]
        [InlineData("6:0", 0.13)]
        [InlineData("7:0", -0.13)]
        [InlineData("8:0", 12.34)]
        [InlineData("9:0", -12.34)]
        [InlineData("10:0", 12.34)]
        public void Should_Parse_Real(string id, double expected)
        {
            // Given, When
            var result = _fixture.Document.TryReadObject(
                PdfObjectId.Parse(id), out var obj);

            // Then
            result.ShouldBeTrue();
            obj.ShouldBeOfType<PdfObjectDefinition>()
                .Object.ShouldBeOfType<PdfReal>()
                .ShouldHaveValue(expected);
        }

        [Fact]
        public void Should_Parse_Null()
        {
            // Given, When
            var result = _fixture.Document.TryReadObject(
                PdfObjectId.Parse("11:0"), out var obj);

            // Then
            result.ShouldBeTrue();
            obj.ShouldBeOfType<PdfObjectDefinition>()
                .Object.ShouldBeOfType<PdfNull>();
        }

        [Theory]
        [InlineData("12:0", "Hello World")]
        [InlineData("13:0", "Hello\nWorld")]
        [InlineData("14:0", "These two strings are the same.")]
        public void Should_Parse_String_Literals(string id, string expected)
        {
            // Given, When
            var result = _fixture.Document.TryReadObject(
                PdfObjectId.Parse(id), out var obj);

            // Then
            result.ShouldBeTrue();
            obj.ShouldBeOfType<PdfObjectDefinition>()
                .Object.ShouldBeOfType<PdfString>()
                .ShouldHaveValue(expected)
                .ShouldHaveEncoding(PdfStringEncoding.Raw);
        }

        [Theory]
        [InlineData("15:0", "PDF")]
        [InlineData("16:0", "PD ")]
        public void Should_Parse_Hex_String_Literals(string id, string expected)
        {
            // Given, When
            var result = _fixture.Document.TryReadObject(
                PdfObjectId.Parse(id), out var obj);

            // Then
            result.ShouldBeTrue();
            obj.ShouldBeOfType<PdfObjectDefinition>()
                .Object.ShouldBeOfType<PdfString>()
                .ShouldHaveValue(expected)
                .ShouldHaveEncoding(PdfStringEncoding.HexLiteral);
        }

        [Theory]
        [InlineData("17:0", "Name1")]
        [InlineData("18:0", "ASomewhatLongerName")]
        [InlineData("19:0", "A;Name_With-Various***Characters?")]
        [InlineData("20:0", "1.2")]
        [InlineData("21:0", "$$")]
        [InlineData("22:0", "@pattern")]
        [InlineData("23:0", ".notdef")]
        [InlineData("24:0", "lime Green")]
        [InlineData("25:0", "paired()parentheses")]
        [InlineData("26:0", "The_Key_of_F#_Minor")]
        [InlineData("27:0", "AB")]
        public void Should_Parse_Name(string id, string expected)
        {
            // Given, When
            var result = _fixture.Document.TryReadObject(
                PdfObjectId.Parse(id), out var obj);

            // Then
            result.ShouldBeTrue();
            obj.ShouldBeOfType<PdfObjectDefinition>()
                .Object.ShouldBeOfType<PdfName>()
                .ShouldHaveValue(expected);
        }

        [Fact]
        public void Should_Parse_Dictionary()
        {
            // Given, When
            var result = _fixture.Document.TryReadObject(
                PdfObjectId.Parse("28:0"), out var obj);

            // Then
            result.ShouldBeTrue();
            obj.ShouldBeOfType<PdfObjectDefinition>()
                .Object.ShouldBeOfType<PdfDictionary>()
                .ShouldHaveKeyValue("Foo", 1)
                .ShouldHaveKeyValue("Bar", 2.1);
        }

        [Fact]
        public void Should_Parse_Array()
        {
            // Given, When
            var result = _fixture.Document.TryReadObject(
                PdfObjectId.Parse("29:0"), out var obj);

            // Then
            result.ShouldBeTrue();
            obj.ShouldBeOfType<PdfObjectDefinition>()
                .Object.ShouldBeOfType<PdfArray>()
                .And(array =>
                {
                    array.Count.ShouldBe(2);
                    array[0].ShouldBeOfType<PdfInteger>().ShouldHaveValue(1);
                    array[1].ShouldBeOfType<PdfReal>().ShouldHaveValue(2.1);
                });
        }

        [Fact]
        public void Should_Parse_Stream()
        {
            // Given, When
            var result = _fixture.Document.TryReadObject(
                PdfObjectId.Parse("30:0"), out var obj);

            // Then
            result.ShouldBeTrue();
            obj.ShouldBeOfType<PdfObjectDefinition>()
                .Object.ShouldBeOfType<PdfStream>()
                .And(stream =>
                {
                    stream.Length.ShouldBe(1);
                    stream.GetData().ShouldBe(new byte[] { 49 });
                });
        }

        [Fact]
        public void Should_Parse_Object_Reference()
        {
            // Given, When
            var result = _fixture.Document.TryReadObject(
                PdfObjectId.Parse("31:0"), out var obj);

            // Then
            result.ShouldBeTrue();
            obj.ShouldBeOfType<PdfObjectDefinition>()
                .Object.ShouldBeOfType<PdfObjectId>()
                .ShouldHaveNumber(8)
                .ShouldHaveGeneration(3);
        }
    }
}