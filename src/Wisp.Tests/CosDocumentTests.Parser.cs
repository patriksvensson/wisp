using JetBrains.Annotations;

namespace Wisp.Tests;

public sealed partial class CosDocumentTests
{
    [UsedImplicitly]
    public sealed class Parsing
    {
        public sealed class Strings
        {
            [Fact]
            public void Escaped_Closing_Parenthesis()
            {
                // Given
                var obj = CosParserFixture.WriteAndParse(
                    new CosDictionary()
                    {
                        { new CosName("C"), new CosName("SC.12.303125") },
                        { new CosName("K"), new CosInteger(73) },
                        { new CosName("Lang"), new CosString(")q") },
                    });

                // Then
                obj.ShouldNotBeNull();
                obj.ShouldBeOfType<CosDictionary>().And()["Lang"]
                    .ShouldBeOfType<CosString>().ShouldHaveValue(")q");
            }

            [Fact]
            public void Escaped_Opening_Parenthesis()
            {
                // Given
                var obj = CosParserFixture.WriteAndParse(
                    new CosDictionary()
                    {
                        { new CosName("C"), new CosName("SC.12.303125") },
                        { new CosName("K"), new CosInteger(73) },
                        { new CosName("Lang"), new CosString("(q") },
                    });

                // Then
                obj.ShouldNotBeNull();
                obj.ShouldBeOfType<CosDictionary>().And()["Lang"]
                    .ShouldBeOfType<CosString>().ShouldHaveValue("(q");
            }

            [Fact]
            public void Escaped_Backslash()
            {
                // Given
                var obj = CosParserFixture.WriteAndParse(
                    new CosDictionary()
                    {
                        { new CosName("Lang"), new CosString("p\\") },
                        { new CosName("C"), new CosName("SC.12.303125") },
                        { new CosName("K"), new CosInteger(73) },
                    });

                // Then
                obj.ShouldNotBeNull();
                obj.ShouldBeOfType<CosDictionary>().And()["Lang"]
                    .ShouldBeOfType<CosString>().ShouldHaveValue("p\\");
            }
        }
    }
}