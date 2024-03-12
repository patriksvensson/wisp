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

    public CosObject? GetObject(ICosObjectCache owner, CosObjectReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);
        return GetObject(owner, reference.Id);
    }

    public CosObject? GetObject(ICosObjectCache owner, CosObjectId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        var xref = _xRefTable.GetXRef(id);
        return GetObjectByXRef(owner, xref);
    }

    private CosObject? GetObjectByXRef(ICosObjectCache owner, CosXRef? xref)
    {
        switch (xref)
        {
            case null:
                return null;

            case CosStreamXRef streamXref:
                // Get the xref to the stream object
                var streamObjectXRef = _xRefTable.GetXRef(streamXref.StreamId) as CosIndirectXRef;
                Debug.Assert(streamObjectXRef != null, "Could not get xref to stream object");

                if (streamObjectXRef.Position == null)
                {
                    throw new InvalidOperationException("Object in object stream should exist in cache");
                }

                // Parse the object stream
                _parser.Seek(streamObjectXRef.Position.Value, SeekOrigin.Begin);
                var streamObject = _parser.Parse() as CosObject;
                Debug.Assert(streamObject != null, "Could not find an object at the stream position");
                var objectStream = streamObject.Object as CosObjectStream;
                Debug.Assert(objectStream != null, "Object was not an object stream");

                // Get the object within the stream
                return objectStream.GetObjectByIndex(owner, streamXref.Index);

            case CosIndirectXRef indirectXRef:
                if (indirectXRef.Position == null)
                {
                    throw new InvalidOperationException("Object should exist in cache");
                }

                _parser.Seek(indirectXRef.Position.Value, SeekOrigin.Begin);
                return _parser.Parse() as CosObject;

            default:
                return null;
        }
    }
}