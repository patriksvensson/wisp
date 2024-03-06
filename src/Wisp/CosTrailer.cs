namespace Wisp;

[PublicAPI]
public sealed class CosTrailer
{
    private readonly CosDictionary _dictionary;

    /// <summary>
    /// Gets or sets the total number of entries in the file's cross-reference table,
    /// as defined by the combination of the original section and all update
    /// sections. Equivalently, this value shall be 1 greater than the
    /// highest object number defined in the file. Any object in a cross-reference
    /// section whose number is greater than this value shall be ignored and
    /// defined to be missing by a conforming reader.
    /// </summary>
    public int Size
    {
        get => _dictionary.GetRequiredInteger(CosNames.Size).IntValue;
        set => _dictionary.Set(CosNames.Size, new CosInteger(value));
    }

    /// <summary>
    /// Gets or sets the byte offset in the decoded stream from the
    /// beginning of the file to the beginning of the previous
    /// cross-reference section.
    /// </summary>
    public long? Prev
    {
        get => _dictionary.GetInt64(CosNames.Prev);
        set => _dictionary.Set(CosNames.Prev, new CosInteger(value));
    }

    /// <summary>
    /// Gets or sets the catalog dictionary for the document
    /// contained in the file.
    /// </summary>
    public CosObjectId Root
    {
        get => _dictionary.GetRequiredObjectId(CosNames.Root);
        set => _dictionary.Set(CosNames.Root, value);
    }

    /// <summary>
    /// Gets or sets the document's encryption dictionary.
    /// </summary>
    public CosDictionary? Encrypt
    {
        get => _dictionary.GetDictionary(CosNames.Encrypt);
        set => _dictionary.Set(CosNames.Encrypt, value);
    }

    /// <summary>
    /// Gets or sets the document's information dictionary.
    /// </summary>
    public CosObjectId Info
    {
        get => _dictionary.GetRequiredObjectId(CosNames.Info);
        set => _dictionary.Set(CosNames.Info, value);
    }

    /// <summary>
    /// Gets or sets an array of two byte-strings
    /// constituting a file identifier.
    /// </summary>
    public CosArray? Id
    {
        get => _dictionary.GetArray(CosNames.Id);
        set => _dictionary.Set(CosNames.Id, value);
    }

    public CosTrailer(CosDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }
}