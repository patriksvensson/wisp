namespace Wisp.Tests;

public sealed class ParserTests
{
    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void Should_Parse_Boolean(string input, bool expected)
    {
        // Given
        var lexer = new Lexer(input.ToStream());
        var parser = new Parser(lexer);

        // When
        var result = parser.ReadObject();

        // Then
        result.ShouldBeOfType<PdfBoolean>()
            .ShouldHaveValue(expected);
    }

    [Theory]
    [InlineData("12", 12)]
    [InlineData("-12", -12)]
    [InlineData("+12", 12)]
    public void Should_Parse_Integer(string input, int expected)
    {
        // Given
        var lexer = new Lexer(input.ToStream());
        var parser = new Parser(lexer);

        // When
        var result = parser.ReadObject();

        // Then
        result.ShouldBeOfType<PdfInteger>()
            .ShouldHaveValue(expected);
    }

    [Theory]
    [InlineData(".13", 0.13)]
    [InlineData("-.13", -0.13)]
    [InlineData("12.34", 12.34)]
    [InlineData("-12.34", -12.34)]
    [InlineData("+12.34", 12.34)]
    public void Should_Parse_Real(string input, double expected)
    {
        // Given
        var lexer = new Lexer(input.ToStream());
        var parser = new Parser(lexer);

        // When
        var result = parser.ReadObject();

        // Then
        result.ShouldBeOfType<PdfReal>()
            .ShouldHaveValue(expected);
    }

    [Fact]
    public void Should_Parse_Null()
    {
        // Given
        var lexer = new Lexer("null".ToStream());
        var parser = new Parser(lexer);

        // When
        var result = parser.ReadObject();

        // Then
        result.ShouldBeOfType<PdfNull>();
    }

    [Theory]
    [InlineData("(Hello World)", "Hello World")]
    [InlineData("(Hello\nWorld)", "Hello\nWorld")]
    [InlineData("(Hello\r\nWorld)", "Hello\r\nWorld")]
    [InlineData("(Hello\r\n(World))", "Hello\r\n(World)")]
    [InlineData("(These \\\ntwo strings \\\nare the same.)", "These two strings are the same.")]
    [InlineData("(These \\\r\ntwo strings \\\r\nare the same.)", "These two strings are the same.")]
    public void Should_Parse_String_Literals(string input, string expected)
    {
        // Given
        var lexer = new Lexer(input.ToStream());
        var parser = new Parser(lexer);

        // When
        var result = parser.ReadObject();

        // Then
        result.ShouldBeOfType<PdfString>()
            .ShouldHaveValue(expected)
            .ShouldHaveEncoding(PdfStringEncoding.Raw);
    }

    [Theory]
    [InlineData("<504446>", "PDF")]
    [InlineData("<50442>", "PD ")]
    public void Should_Parse_Hex_String_Literals(string input, string expected)
    {
        // Given
        var lexer = new Lexer(input.ToStream());
        var parser = new Parser(lexer);

        // When
        var result = parser.ReadObject();

        // Then
        result.ShouldBeOfType<PdfString>()
            .ShouldHaveValue(expected)
            .ShouldHaveEncoding(PdfStringEncoding.HexLiteral);
    }

    [Theory]
    [InlineData("/Name1", "Name1")]
    [InlineData("/ASomewhatLongerName", "ASomewhatLongerName")]
    [InlineData("/A;Name_With-Various***Characters?", "A;Name_With-Various***Characters?")]
    [InlineData("/1.2", "1.2")]
    [InlineData("/$$", "$$")]
    [InlineData("/@pattern", "@pattern")]
    [InlineData("/.notdef", ".notdef")]
    [InlineData("/lime#20Green", "lime Green")]
    [InlineData("/paired#28#29parentheses", "paired()parentheses")]
    [InlineData("/The_Key_of_F#23_Minor", "The_Key_of_F#_Minor")]
    [InlineData("/A#42", "AB")]
    public void Should_Parse_Name(string input, string expected)
    {
        // Given
        var lexer = new Lexer(input.ToStream());
        var parser = new Parser(lexer);

        // When
        var result = parser.ReadObject();

        // Then
        result.ShouldBeOfType<PdfName>()
            .ShouldHaveValue(expected);
    }

    [Fact]
    public void Should_Parse_Dictionary()
    {
        // Given
        var input = "<</Foo 1 /Bar 2.1>>";
        var lexer = new Lexer(input.ToStream());
        var parser = new Parser(lexer);

        // When
        var result = parser.ReadObject();

        // Then
        result.ShouldBeOfType<PdfDictionary>().And(dictionary =>
        {
            dictionary["Foo"]
                .ShouldBeOfType<PdfInteger>()
                .ShouldHaveValue(1);

            dictionary["Bar"]
                .ShouldBeOfType<PdfReal>()
                .ShouldHaveValue(2.1);
        });
    }

    [Fact]
    public void Should_Parse_Array()
    {
        // Given
        var input = "[ 1 2.1 ]";
        var lexer = new Lexer(input.ToStream());
        var parser = new Parser(lexer);

        // When
        var result = parser.ReadObject();

        // Then
        result.ShouldBeOfType<PdfArray>().And(obj =>
        {
            var items = obj.ToList();

            items.Count.ShouldBe(2);
            items[0].ShouldBeOfType<PdfInteger>().ShouldHaveValue(1);
            items[1].ShouldBeOfType<PdfReal>().ShouldHaveValue(2.1);
        });
    }

    [Fact]
    public void Should_Parse_Stream()
    {
        // Given
        var input = "<</Length 1 >>\nstream\n1\nendstream";
        var lexer = new Lexer(input.ToStream());
        var parser = new Parser(lexer);

        // When
        var result = parser.ReadObject();

        // Then
        result.ShouldBeOfType<PdfStream>().And(stream =>
        {
            stream.Length.ShouldBe(1);
            stream.GetData().ShouldBe(new byte[] { 49 });
        });
    }

    [Fact]
    public void Should_Parse_Object_Reference()
    {
        // Given
        var input = "3 7 R";
        var lexer = new Lexer(input.ToStream());
        var parser = new Parser(lexer);

        // When
        var result = parser.ReadObject();

        // Then
        result.ShouldBeOfType<PdfObjectId>().And(reference =>
        {
            reference.Number.ShouldBe(3);
            reference.Generation.ShouldBe(7);
        });
    }

    [Fact]
    public void Should_Parse_Object_Definition()
    {
        // Given
        var input = "3 7 obj\n<</Length 1 >>\nstream\n1\nendstream";
        var lexer = new Lexer(input.ToStream());
        var parser = new Parser(lexer);

        // When
        var result = parser.ReadObject();

        // Then
        result.ShouldBeOfType<PdfObjectDefinition>().And(reference =>
        {
            reference.Id.Number.ShouldBe(3);
            reference.Id.Generation.ShouldBe(7);

            reference.Object.ShouldBeOfType<PdfStream>().And(stream =>
            {
                stream.Length.ShouldBe(1);
                stream.GetData().ShouldBe(new byte[] { 49 });
            });
        });
    }
}