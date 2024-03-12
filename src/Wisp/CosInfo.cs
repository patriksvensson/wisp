namespace Wisp;

[PublicAPI]
public sealed class CosInfo : CosObjectReference<CosDictionary>
{
    /// <summary>
    /// Gets or sets the document's title.
    /// </summary>
    public CosString? Title
    {
        get => this.Object.GetString(CosNames.Title);
        set => this.Object.Set(CosNames.Title, value);
    }

    /// <summary>
    /// Gets or sets the name of the person who created the document.
    /// </summary>
    public CosString? Author
    {
        get => this.Object.GetString(CosNames.Author);
        set => this.Object.Set(CosNames.Author, value);
    }

    /// <summary>
    /// Gets or sets the subject of the document.
    /// </summary>
    public CosString? Subject
    {
        get => this.Object.GetString(CosNames.Subject);
        set => this.Object.Set(CosNames.Subject, value);
    }

    /// <summary>
    /// Gets or sets the keywords associated with the document.
    /// </summary>
    public CosString? Keywords
    {
        get => this.Object.GetString(CosNames.Keywords);
        set => this.Object.Set(CosNames.Keywords, value);
    }

    /// <summary>
    /// Gets or sets the name of the conforming product that created the
    /// original document, if the document was converted to PDF from
    /// another format.
    /// </summary>
    public CosString? Creator
    {
        get => this.Object.GetString(CosNames.Creator);
        set => this.Object.Set(CosNames.Creator, value);
    }

    /// <summary>
    /// Gets or sets the name of the conforming product that converted
    /// it to PDF, if the document was converted to PDF from another format.
    /// </summary>
    public CosString? Producer
    {
        get => this.Object.GetString(CosNames.Producer);
        set => this.Object.Set(CosNames.Producer, value);
    }

    /// <summary>
    /// Gets or sets the date and time the document was created.
    /// </summary>
    public CosDate? CreationDate
    {
        get => this.Object.GetDate(CosNames.CreationDate);
        set => this.Object.Set(CosNames.CreationDate, value);
    }

    /// <summary>
    /// Gets or sets the date and time the document
    /// was most recently modified.
    /// </summary>
    public CosDate? ModDate
    {
        get => this.Object.GetDate(CosNames.ModDate);
        set => this.Object.Set(CosNames.ModDate, value);
    }

    public CosInfo(CosObject obj)
        : base(obj)
    {
    }

    internal CosInfo(CosObjectReference id, CosDictionary dictionary)
        : base(id, dictionary)
    {
    }
}