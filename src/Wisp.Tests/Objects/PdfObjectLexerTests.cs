namespace Wisp.Tests;

public sealed class PdfObjectLexerTests
{
    [Fact]
    public void Should_Read_Comment()
    {
        // Given
        var lexer = new PdfObjectLexer("%Hello World".ToStream());

        // When
        var result = lexer.TryRead(out var output);

        // Then
        result.ShouldBeTrue();
        output.ShouldBeComment();
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
    public void Should_Read_Names(string input, string expected)
    {
        // Given
        var lexer = new PdfObjectLexer(input.ToStream());

        // When
        var result = lexer.TryRead(out var output);

        // Then
        result.ShouldBeTrue();
        output.ShouldBeName(expected);
    }

    [Theory]
    [InlineData("(Hello World)", "Hello World")]
    [InlineData("(Hello\nWorld)", "Hello\nWorld")]
    [InlineData("(Hello\r\nWorld)", "Hello\r\nWorld")]
    [InlineData("(Hello\r\n(World))", "Hello\r\n(World)")]
    [InlineData("(These \\\ntwo strings \\\nare the same.)", "These two strings are the same.")]
    [InlineData("(These \\\r\ntwo strings \\\r\nare the same.)", "These two strings are the same.")]
    public void Should_Read_String_Literals(string input, string expected)
    {
        // Given
        var lexer = new PdfObjectLexer(input.ToStream());

        // When
        var result = lexer.TryRead(out var output);

        // Then
        result.ShouldBeTrue();
        output.ShouldBeStringLiteral(expected);
    }

    [Theory]
    [InlineData("<504446>", "PDF")]
    [InlineData("<50442>", "PD ")]
    public void Should_Read_Hex_String_Literals(string input, string expected)
    {
        // Given
        var lexer = new PdfObjectLexer(input.ToStream());

        // When
        var result = lexer.TryRead(out var output);

        // Then
        result.ShouldBeTrue();
        output.ShouldBeHexStringLiteral(expected);
    }

    [Fact]
    public void Should_Read_Begin_Dictionary()
    {
        // Given
        var lexer = new PdfObjectLexer("<</Foo 1 /Bar 2.1>>".ToStream());

        // When
        var result = lexer.TryRead(out var output);

        // Then
        result.ShouldBeTrue();
        output?.Kind.ShouldBe(PdfObjectTokenKind.BeginDictionary);
    }

    [Fact]
    public void Should_Read_End_Dictionary()
    {
        // Given
        var lexer = new PdfObjectLexer(">>".ToStream());

        // When
        var result = lexer.TryRead(out var output);

        // Then
        result.ShouldBeTrue();
        output?.Kind.ShouldBe(PdfObjectTokenKind.EndDictionary);
    }

    [Fact]
    public void Should_Read_Begin_Array()
    {
        // Given
        var lexer = new PdfObjectLexer("[".ToStream());

        // When
        var result = lexer.TryRead(out var output);

        // Then
        result.ShouldBeTrue();
        output?.Kind.ShouldBe(PdfObjectTokenKind.BeginArray);
    }

    [Fact]
    public void Should_Read_End_Array()
    {
        // Given
        var lexer = new PdfObjectLexer("]".ToStream());

        // When
        var result = lexer.TryRead(out var output);

        // Then
        result.ShouldBeTrue();
        output?.Kind.ShouldBe(PdfObjectTokenKind.EndArray);
    }

    [Theory]
    [InlineData("12", 12)]
    [InlineData("-12", -12)]
    [InlineData("+12", 12)]
    public void Should_Parse_Integers(string input, int expected)
    {
        // Given
        var lexer = new PdfObjectLexer(input.ToStream());

        // When
        var result = lexer.TryRead(out var output);

        // Then
        result.ShouldBeTrue();
        output.ShouldBeInteger(expected);
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
        var lexer = new PdfObjectLexer(input.ToStream());

        // When
        var result = lexer.TryRead(out var output);

        // Then
        result.ShouldBeTrue();
        output.ShouldBeReal(expected);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    public void Should_Parse_Boolean(string input)
    {
        // Given
        var lexer = new PdfObjectLexer(input.ToStream());

        // When
        var result = lexer.TryRead(out var output);

        // Then
        result.ShouldBeTrue();
        output?.Kind.ShouldBe(PdfObjectTokenKind.Boolean);
        output?.Text.ShouldBe(input);
    }

    [Theory]
    [InlineData("trailer", PdfObjectTokenKind.Trailer)]
    [InlineData("obj", PdfObjectTokenKind.BeginObject)]
    [InlineData("endobj", PdfObjectTokenKind.EndObject)]
    [InlineData("stream", PdfObjectTokenKind.BeginStream)]
    [InlineData("endstream", PdfObjectTokenKind.EndStream)]
    [InlineData("null", PdfObjectTokenKind.Null)]
    [InlineData("R", PdfObjectTokenKind.Reference)]
    [InlineData("startxref", PdfObjectTokenKind.StartXRef)]
    [InlineData("xref", PdfObjectTokenKind.XRef)]
    public void Should_Parse_Keyword(string input, PdfObjectTokenKind expected)
    {
        // Given
        var lexer = new PdfObjectLexer(input.ToStream());

        // When
        var result = lexer.TryRead(out var output);

        // Then
        result.ShouldBeTrue();
        output?.Kind.ShouldBe(expected);
    }
}