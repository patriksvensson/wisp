namespace Wisp.Cos;

[PublicAPI]
public sealed class CosObjectResolver
{
    private readonly CosParser _parser;
    private readonly CosXRefTable _xRefTable;

    public CosObjectResolver(CosParser parser, CosXRefTable xRefTable)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _xRefTable = xRefTable ?? throw new ArgumentNullException(nameof(xRefTable));
    }

    public CosObject? GetObject(CosObjectId id)
    {
        var xref = _xRefTable.GetXRef(id);
        switch (xref)
        {
            case null:
                return null;

            case CosStreamXRef streamXref:
                // Get the xref to the stream object
                var streamObjectXRef = _xRefTable.GetXRef(streamXref.StreamId) as CosIndirectXRef;
                Debug.Assert(streamObjectXRef != null, "Could not get xref to stream object");

                // Parse the object stream
                _parser.Seek(streamObjectXRef.Position, SeekOrigin.Begin);
                var streamObject = _parser.ParseObject() as CosObject;
                Debug.Assert(streamObject != null, "Could not find an object at the stream position");
                var objectStream = streamObject.Object as CosObjectStream;
                Debug.Assert(objectStream != null, "Object was not an object stream");

                // Get the object within the stream
                return objectStream.GetObjectByIndex(streamXref.Index);

            case CosIndirectXRef indirectXRef:
                _parser.Seek(indirectXRef.Position, SeekOrigin.Begin);
                return _parser.ParseObject() as CosObject;

            default:
                return null;
        }
    }
}