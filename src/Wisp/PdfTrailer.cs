namespace Wisp;

public sealed class PdfTrailer
{
    private readonly PdfDictionary _dictionary;

    public PdfInteger Size => _dictionary.GetRequiredValue<PdfInteger>(PdfName.Known.Size);
    public PdfInteger? Prev => _dictionary.GetOptionalValue<PdfInteger>(PdfName.Known.Prev);
    public PdfObjectId Root => _dictionary.GetRequiredValue<PdfObjectId>(PdfName.Known.Root);
    public PdfObjectId? Encrypt => _dictionary.GetOptionalValue<PdfObjectId>(PdfName.Known.Encrypt);
    public PdfObjectId? Info => _dictionary.GetOptionalValue<PdfObjectId>(PdfName.Known.Info);
    public PdfArray? Id => _dictionary.GetOptionalValue<PdfArray>(PdfName.Known.Id);

    public PdfTrailer(PdfDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }
}