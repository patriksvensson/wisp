namespace Wisp;

public enum PdfObjectTokenKind
{
    Unknown = 0,
    Comment,
    Name,
    StringLiteral,
    BeginDictionary,
    HexStringLiteral,
    BeginArray,
    EndArray,
    EndDictionary,
    Real,
    Integer,
    Boolean,
    Trailer,
    BeginObject,
    EndObject,
    BeginStream,
    EndStream,
    Null,
    Reference,
    StartXRef,
    XRef,
    Keyword,
}