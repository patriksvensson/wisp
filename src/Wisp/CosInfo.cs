namespace Wisp;

public sealed class CosInfo : CosDictionary
{
    /// <summary>
    /// Gets the COS object ID of the info dictionary,
    /// if the document was opened from a file.
    /// </summary>
    public CosObjectId? Id { get; }

    /// <summary>
    /// Gets or sets the document's title.
    /// </summary>
    public CosString? Title
    {
        get => this.GetString(CosNames.Title);
        set => this.Set(CosNames.Title, value);
    }

    /// <summary>
    /// Gets or sets the name of the person who created the document.
    /// </summary>
    public CosString? Author
    {
        get => this.GetString(CosNames.Author);
        set => this.Set(CosNames.Author, value);
    }

    /// <summary>
    /// Gets or sets the subject of the document.
    /// </summary>
    public CosString? Subject
    {
        get => this.GetString(CosNames.Subject);
        set => this.Set(CosNames.Subject, value);
    }

    /// <summary>
    /// Gets or sets the keywords associated with the document.
    /// </summary>
    public CosString? Keywords
    {
        get => this.GetString(CosNames.Keywords);
        set => this.Set(CosNames.Keywords, value);
    }

    /// <summary>
    /// Gets or sets the name of the conforming product that created the
    /// original document, if the document was converted to PDF from
    /// another format.
    /// </summary>
    public CosString? Creator
    {
        get => this.GetString(CosNames.Creator);
        set => this.Set(CosNames.Creator, value);
    }

    /// <summary>
    /// Gets or sets the name of the conforming product that converted
    /// it to PDF, if the document was converted to PDF from another format.
    /// </summary>
    public CosString? Producer
    {
        get => this.GetString(CosNames.Producer);
        set => this.Set(CosNames.Producer, value);
    }

    /// <summary>
    /// Gets or sets the date and time the document was created.
    /// </summary>
    public CosDate? CreationDate
    {
        get => this.GetDate(CosNames.CreationDate);
        set => this.Set(CosNames.CreationDate, value);
    }

    /// <summary>
    /// Gets or sets the date and time the document
    /// was most recently modified.
    /// </summary>
    public CosDate? ModDate
    {
        get => this.GetDate(CosNames.ModDate);
        set => this.Set(CosNames.ModDate, value);
    }

    public CosInfo()
    {
    }

    internal CosInfo(CosObjectId id, CosDictionary dictionary)
        : base(dictionary)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
    }
}