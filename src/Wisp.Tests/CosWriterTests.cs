namespace Wisp.Tests;

public sealed class CosWriterTests
{
    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void Should_Write_Booleans_Correctly(bool value, string expected)
    {
        // Given
        var fixture = new CosWriterFixture();

        // When
        fixture.Write(new CosBoolean(value));

        // Then
        fixture.Result.ShouldBe(expected);
    }

    [Fact]
    public void Should_Write_Dates_Correctly()
    {
        // Given
        var fixture = new CosWriterFixture();

        // When
        fixture.Write(new CosDate(new DateTimeOffset(2024, 3, 7, 23, 31, 12, TimeSpan.FromHours(1))));

        // Then
        fixture.Result.ShouldBe("(D:20240307233112+01'00)");
    }

    [Fact]
    public void Should_Write_Arrays_Correctly()
    {
        // Given
        var fixture = new CosWriterFixture();

        // When
        fixture.Write(new CosArray(
            new[]
            {
                new CosBoolean(true),
                new CosBoolean(false),
            }));

        // Then
        fixture.Result.ShouldBe("[true false]");
    }

    [Fact]
    public void Should_Write_Dictionaries_Correctly()
    {
        // Given
        var fixture = new CosWriterFixture();

        // When
        fixture.Write(new CosDictionary
        {
            { new CosName("Size"), new CosInteger(40) },
            { new CosName("Root"), new CosObjectReference(25, 0) },
        });

        // Then
        fixture.Result.ShouldBe(
            """
            <<
            /Size 40
            /Root 25 0 R
            >>
            """);
    }

    [Fact]
    public void Should_Write_Integers_Correctly()
    {
        // Given
        var fixture = new CosWriterFixture();

        // When
        fixture.Write(new CosInteger(32));

        // Then
        fixture.Result.ShouldBe("32");
    }

    [Theory]
    [InlineData("Hello", "/Hello")]
    [InlineData("/Hello", "/Hello")]
    public void Should_Write_Names_Correctly(string name, string expected)
    {
        // Given
        var fixture = new CosWriterFixture();

        // When
        fixture.Write(new CosName(name));

        // Then
        fixture.Result.ShouldBe(expected);
    }

    [Fact]
    public void Should_Write_Null_Correctly()
    {
        // Given
        var fixture = new CosWriterFixture();

        // When
        fixture.Write(new CosNull());

        // Then
        fixture.Result.ShouldBe("null");
    }

    [Fact]
    public void Should_Write_Real_Numbers_Correctly()
    {
        // Given
        var fixture = new CosWriterFixture();

        // When
        fixture.Write(new CosReal(32.33));

        // Then
        fixture.Result.ShouldBe("32.33");
    }

    [Fact]
    public void Should_Write_Hex_Strings_Correctly()
    {
        // Given
        var fixture = new CosWriterFixture();

        // When
        fixture.Write(new CosHexString([80, 97, 116, 114, 105, 107]));

        // Then
        fixture.Result.ShouldBe("<50617472696B>");
    }

    [Fact]
    public void Should_Write_Object_References_Correctly()
    {
        // Given
        var fixture = new CosWriterFixture();

        // When
        fixture.Write(new CosObjectReference(32, 2));

        // Then
        fixture.Result.ShouldBe("32 2 R");
    }

    [Fact]
    public void Should_Write_Strings_Correctly()
    {
        // Given
        var fixture = new CosWriterFixture();

        // When
        fixture.Write(new CosString("Hello World"));

        // Then
        fixture.Result.ShouldBe("(Hello World)");
    }

    [Fact]
    public void Should_Write_Objects_Correctly()
    {
        // Given
        var fixture = new CosWriterFixture();

        // When
        fixture.Write(new CosObject(
            new CosObjectId(12, 0),
            new CosString("Hello World")));

        // Then
        fixture.Result.ShouldBe(
            """
            12 0 obj
            (Hello World)
            endobj
            """);
    }
}