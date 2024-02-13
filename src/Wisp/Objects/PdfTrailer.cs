namespace Wisp.Objects;

public sealed class PdfTrailer
{
    private readonly PdfDictionary _dictionary;

    public PdfInteger Size
    {
        get => _dictionary.GetRequiredValue<PdfInteger>(PdfName.Known.Size);
        set => _dictionary.Set(PdfName.Known.Size, value);
    }

    public PdfInteger? Prev
    {
        get => _dictionary.GetOptionalValue<PdfInteger>(PdfName.Known.Prev);
        set => _dictionary.SetIfNotNull(PdfName.Known.Prev, value);
    }

    public PdfObjectId Root
    {
        get => _dictionary.GetRequiredValue<PdfObjectId>(PdfName.Known.Root);
        set => _dictionary.Set(PdfName.Known.Root, value);
    }

    public PdfObjectId? Encrypt
    {
        get => _dictionary.GetOptionalValue<PdfObjectId>(PdfName.Known.Encrypt);
        set => _dictionary.SetIfNotNull(PdfName.Known.Encrypt, value);
    }

    public PdfObjectId? Info
    {
        get => _dictionary.GetOptionalValue<PdfObjectId>(PdfName.Known.Info);
        set => _dictionary.SetIfNotNull(PdfName.Known.Info, value);
    }

    public PdfArray? Id
    {
        get => _dictionary.GetOptionalValue<PdfArray>(PdfName.Known.Id);
        set => _dictionary.SetIfNotNull(PdfName.Known.Id, value);
    }

    public PdfTrailer(PdfDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }
}