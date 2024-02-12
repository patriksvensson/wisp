namespace Wisp;

internal interface IPdfReaderContext
{
    PdfObjectCache Cache { get; }
    PdfXRefTable XRefTable { get; }
}