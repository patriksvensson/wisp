namespace Wisp.Internal;

internal interface IPdfReaderContext
{
    PdfObjectCache Cache { get; }
    PdfXRefTable XRefTable { get; }
}