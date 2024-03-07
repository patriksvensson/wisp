namespace Wisp;

[PublicAPI]
public sealed class CosTrailer : CosDictionary
{
    /// <summary>
    /// Gets or sets the total number of entries in the file's cross-reference table,
    /// as defined by the combination of the original section and all update
    /// sections. Equivalently, this value shall be 1 greater than the
    /// highest object number defined in the file. Any object in a cross-reference
    /// section whose number is greater than this value shall be ignored and
    /// defined to be missing by a conforming reader.
    /// </summary>
    public CosInteger Size
    {
        get => this.GetRequiredInteger(CosNames.Size);
        set => this.Set(CosNames.Size, value);
    }

    /// <summary>
    /// Gets or sets the byte offset in the decoded stream from the
    /// beginning of the file to the beginning of the previous
    /// cross-reference section.
    /// </summary>
    public long? Prev
    {
        get => this.GetInt64(CosNames.Prev);
        set => this.Set(CosNames.Prev, new CosInteger(value));
    }

    /// <summary>
    /// Gets or sets the catalog dictionary for the document
    /// contained in the file.
    /// </summary>
    public CosObjectId Root
    {
        get => this.GetRequiredObjectId(CosNames.Root);
        set => this.Set(CosNames.Root, value);
    }

    /// <summary>
    /// Gets or sets the document's encryption dictionary.
    /// </summary>
    public CosDictionary? Encrypt
    {
        get => this.GetDictionary(CosNames.Encrypt);
        set => this.Set(CosNames.Encrypt, value);
    }

    /// <summary>
    /// Gets or sets the document's information dictionary.
    /// </summary>
    public CosObjectId? Info
    {
        get => this.GetObjectId(CosNames.Info);
        set => this.Set(CosNames.Info, value);
    }

    /// <summary>
    /// Gets or sets an array of two byte-strings
    /// constituting a file identifier.
    /// </summary>
    public CosArray? Id
    {
        get => this.GetArray(CosNames.Id);
        set => this.Set(CosNames.Id, value);
    }

    public CosTrailer(CosDictionary dictionary)
        : base(dictionary)
    {
    }
}