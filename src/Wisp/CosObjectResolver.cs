namespace Wisp;

internal sealed class CosObjectResolver : IDisposable
{
    private readonly CosParser _parser;
    private readonly CosXRefTable _xRefTable;

    public CosObjectResolver(CosParser parser, CosXRefTable xRefTable)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _xRefTable = xRefTable ?? throw new ArgumentNullException(nameof(xRefTable));
    }

    public void Dispose()
    {
        _parser.Dispose();
    }

    public (CosObject? Owner, CosObject Object)? GetObject(ICosObjectCache cache, CosObjectId id)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(id);

        switch (_xRefTable.GetXRef(id))
        {
            case null:
                return null;

            case CosStreamXRef streamXref:
                // Get the xref to the stream object
                var streamObjectXRef = _xRefTable.GetXRef(streamXref.StreamId) as CosIndirectXRef;
                if (streamObjectXRef == null)
                {
                    throw new WispObjectResolveException(
                        _parser, "Could not get xref to stream object");
                }

                // Ensure that the stream has a position
                if (streamObjectXRef.Position == null)
                {
                    throw new WispObjectResolveException(
                        _parser, "Object in object stream should exist in cache");
                }

                // Is the stream itself in the cache?
                // Don't try to resolve the stream object; that will lead to a stack overflow
                var ownerObject = cache.Get(streamObjectXRef.Id, CosResolveFlags.NoResolve);
                if (ownerObject == null)
                {
                    // Parse the object stream
                    _parser.Seek(streamObjectXRef.Position.Value, SeekOrigin.Begin);
                    ownerObject = _parser.Parse() as CosObject;
                    if (ownerObject == null)
                    {
                        throw new WispObjectResolveException(
                            _parser, "Could not find an object at the stream position");
                    }
                }

                // Ensure the primitive is an object stream
                var objectStream = ownerObject.Object as CosObjectStream;
                if (objectStream == null)
                {
                    throw new WispObjectResolveException(
                        _parser, "Object was not an object stream");
                }

                // Get the object within the stream
                var objectStreamItem = objectStream.GetObjectByIndex(cache, streamXref.Index);
                if (objectStreamItem == null)
                {
                    throw new WispObjectResolveException(
                        _parser, $"Could not get object in object stream at index {streamXref.Index}");
                }

                return (ownerObject, objectStreamItem);

            case CosIndirectXRef indirectXRef:
                if (indirectXRef.Position == null)
                {
                    throw new WispObjectResolveException(
                        _parser, "Object should exist in cache (no position)");
                }

                _parser.Seek(indirectXRef.Position.Value, SeekOrigin.Begin);
                var result = _parser.Parse() as CosObject;
                if (result == null)
                {
                    return null;
                }

                return (null, result);
            default:
                return null;
        }
    }
}