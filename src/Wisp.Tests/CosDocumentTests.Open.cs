using Wisp.Security;

namespace Wisp.Tests;

public sealed partial class CosDocumentTests
{
    [ExpectationPath("Open")]
    public sealed class Open
    {
        [Fact]
        [Expectation("Unpack")]
        public Task Should_Unpack_Object_Streams_If_Specified()
        {
            // Given, When
            var document = CosDocumentFixture.Simple.Create(
                new CosReaderSettings
                {
                    UnpackObjectStreams = true,
                }).Document;

            // Then
            return CosVerifier.Verify(document);
        }

        [Fact]
        [Expectation("NoUnpacking")]
        public Task Should_Not_Unpack_Object_Streams_If_Not_Specified()
        {
            // Given, When
            var document = CosDocumentFixture.Simple.Create(
                new CosReaderSettings
                {
                    UnpackObjectStreams = false,
                }).Document;

            // Then
            return CosVerifier.Verify(
                document,
                new CosSerializerSettings
                {
                    ExpandStreamObjects = false,
                });
        }

        [Fact]
        public void Should_Open_Encrypted_Document()
        {
            // Given, When
            var stream = EmbeddedResourceReader.GetStream(
                CosDocumentFixture.SimpleEncrypted.Path);

            // When
            var document = CosDocument.Open(stream);

            // Then
        }
    }
}