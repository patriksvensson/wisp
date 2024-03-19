namespace Wisp.Tests;

public sealed partial class CosDocumentTests
{
    public sealed class Parsing
    {
        public sealed class Strings
        {
            [Fact]
            public void Should_Respect_Escaped_Closing_Parentheses_In_Strings()
            {
                // Given
                var doc = new CosDocument();
                doc.Objects.Set(new CosObject(new CosObjectId(1, 0), new CosDictionary()
                {
                    { new CosName("C"), new CosName("SC.12.303125") },
                    { new CosName("K"), new CosInteger(73) },
                    { new CosName("Lang"), new CosString(")q") },
                }));
                var stream = new MemoryStream();
                doc.Save(stream, leaveOpen: true);

                // When
                var newDocument = CosDocument.Open(stream);
                var obj = newDocument.Objects.Get(new CosObjectId(1, 0));

                // Then
                obj.ShouldNotBeNull();
                obj.Object.ShouldBeOfType<CosDictionary>().And()["Lang"]
                    .ShouldBeOfType<CosString>().ShouldHaveValue(")q");
            }

            [Fact]
            public void Should_Respect_Escaped_Opening_Parentheses_In_Strings()
            {
                // Given
                var doc = new CosDocument();
                doc.Objects.Set(new CosObject(new CosObjectId(1, 0), new CosDictionary()
                {
                    { new CosName("C"), new CosName("SC.12.303125") },
                    { new CosName("K"), new CosInteger(73) },
                    { new CosName("Lang"), new CosString("(q") },
                }));
                var stream = new MemoryStream();
                doc.Save(stream, leaveOpen: true);

                // When
                var newDocument = CosDocument.Open(stream);
                var obj = newDocument.Objects.Get(new CosObjectId(1, 0));

                // Then
                obj.ShouldNotBeNull();
                obj.Object.ShouldBeOfType<CosDictionary>().And()["Lang"]
                    .ShouldBeOfType<CosString>().ShouldHaveValue("(q");
            }

            [Fact]
            public void Escaped_Backslash()
            {
                // Given
                var doc = new CosDocument();
                doc.Objects.Set(new CosObject(new CosObjectId(1, 0), new CosDictionary()
                {
                    { new CosName("Lang"), new CosString("p\\") },
                    { new CosName("C"), new CosName("SC.12.303125") },
                    { new CosName("K"), new CosInteger(73) },
                }));
                var stream = new MemoryStream();
                doc.Save(stream, leaveOpen: true);

                // When
                var newDocument = CosDocument.Open(stream);
                var obj = newDocument.Objects.Get(new CosObjectId(1, 0));

                // Then
                obj.ShouldNotBeNull();
                obj.Object.ShouldBeOfType<CosDictionary>().And()["Lang"]
                    .ShouldBeOfType<CosString>().ShouldHaveValue("p\\");
            }
        }
    }
}