namespace Wisp.Testing.Fixtures;

public static class CosParserFixture
{
    public static ICosPrimitive? WriteAndParse(ICosPrimitive primitive)
    {
        var doc = new CosDocument();
        doc.Objects.Set(new CosObject(new CosObjectId(1, 0), primitive));
        using var stream = new MemoryStream();
        doc.Save(stream, new CosWriterSettings
        {
            LeaveStreamOpen = true,
        });

        // When
        var newDocument = CosDocument.Open(stream);
        return newDocument.Objects.Get(new CosObjectId(1, 0))?.Object;
    }
}