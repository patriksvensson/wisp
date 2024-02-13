namespace Wisp.Internal;

internal sealed class PdfObjectParser : IDisposable
{
    public PdfObjectLexer Lexer { get; }
    public bool IsStreamObject { get; }

    public PdfObjectParser(Stream stream, bool isStreamObject = false)
    {
        Lexer = new PdfObjectLexer(stream);
        IsStreamObject = isStreamObject;
    }

    public PdfObjectParser(PdfObjectLexer lexer)
    {
        Lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
    }

    public void Dispose()
    {
        Lexer.Dispose();
    }

    public PdfObject ParseObject()
    {
        while (Lexer.Check(PdfObjectTokenKind.Comment))
        {
            Lexer.Read();
        }

        if (!Lexer.Peek(out var token))
        {
            throw new InvalidOperationException("Reached end of buffer");
        }

        return token.Kind switch
        {
            PdfObjectTokenKind.Null => ParseNull(),
            PdfObjectTokenKind.Boolean => ParseBoolean(),
            PdfObjectTokenKind.Integer => ParseInteger(),
            PdfObjectTokenKind.Real => ParseReal(),
            PdfObjectTokenKind.StringLiteral => ParseStringLiteral(),
            PdfObjectTokenKind.HexStringLiteral => ParseHexStringLiteral(),
            PdfObjectTokenKind.Name => ParseName(),
            PdfObjectTokenKind.BeginDictionary => ParseDictionary(),
            PdfObjectTokenKind.BeginArray => ParseArray(),
            PdfObjectTokenKind.XRef => ParseXRefTable(),
            _ => throw new InvalidOperationException($"Unknown token {token.Kind} encountered in stream"),
        };
    }

    private PdfObject ParseBoolean()
    {
        var token = Lexer.Expect(PdfObjectTokenKind.Boolean);
        return (token.Text == "true")
            ? new PdfBoolean(true)
            : new PdfBoolean(false);
    }

    private PdfObject ParseInteger()
    {
        var value = Lexer.Expect(PdfObjectTokenKind.Integer).ParseInteger();
        var position = Lexer.Reader.Position;

        // Got an integer next?
        if (Lexer.Peek(out var token) && token.Kind == PdfObjectTokenKind.Integer)
        {
            var generation = Lexer.Expect(PdfObjectTokenKind.Integer).ParseInteger();

            if (Lexer.Peek(out token))
            {
                if (token.Kind == PdfObjectTokenKind.Reference)
                {
                    // Reference means object ID
                    Lexer.Expect(PdfObjectTokenKind.Reference);
                    return new PdfObjectId(value, generation);
                }
                else if (token.Kind == PdfObjectTokenKind.BeginObject)
                {
                    // Object definition
                    Lexer.Expect(PdfObjectTokenKind.BeginObject);
                    return new PdfObjectDefinition(
                        new PdfObjectId(value, generation),
                        ParseObject());
                }
            }

            // Rewind the reader
            Lexer.Reader.Seek(position, SeekOrigin.Begin);
        }

        return new PdfInteger(value);
    }

    private PdfObject ParseReal()
    {
        var value = Lexer.Expect(PdfObjectTokenKind.Real).ParseReal();
        return new PdfReal(value);
    }

    private PdfObject ParseNull()
    {
        Lexer.Expect(PdfObjectTokenKind.Null);
        return new PdfNull();
    }

    private PdfObject ParseStringLiteral()
    {
        static bool DecodeString(
            byte[] bytes,
            [NotNullWhen(true)] out string? decoded,
            [NotNullWhen(true)] out PdfStringEncoding? encoding)
        {
            // Big endian unicode?
            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            {
                decoded = Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);
                encoding = PdfStringEncoding.BigEndianUnicode;
                return true;
            }

            // Little endian unicode?
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            {
                decoded = Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
                encoding = PdfStringEncoding.Unicode;
                return true;
            }

            // Treat everything else as raw.
            decoded = Encoding.UTF8.GetString(bytes);
            encoding = PdfStringEncoding.Raw;
            return true;
        }

        var token = Lexer.Expect(PdfObjectTokenKind.StringLiteral);
        if (token.Lexeme == null)
        {
            throw new InvalidOperationException("String literal token had no byte content");
        }

        if (!DecodeString(token.Lexeme, out var decoded, out var encoding))
        {
            throw new InvalidOperationException("Could not decode PDF string");
        }

        return new PdfString(decoded, encoding.Value);
    }

    private PdfObject ParseHexStringLiteral()
    {
        var token = Lexer.Expect(PdfObjectTokenKind.HexStringLiteral);
        return new PdfString(token.Text!, PdfStringEncoding.HexLiteral);
    }

    private PdfObject ParseName()
    {
        var token = Lexer.Expect(PdfObjectTokenKind.Name);
        return new PdfName(token.Text!);
    }

    private PdfObject ParseDictionary()
    {
        Lexer.Expect(PdfObjectTokenKind.BeginDictionary);

        var result = new PdfDictionary();
        while (Lexer.Peek(out var token))
        {
            if (token.Kind == PdfObjectTokenKind.EndDictionary)
            {
                break;
            }

            var temp = ParseObject();
            if (temp is not PdfName key)
            {
                throw new InvalidOperationException("Encountered dictionary key that was not a PDF name");
            }

            var value = ParseObject();
            result.Set(key, value);
        }

        if (IsStreamObject)
        {
            if (Lexer.Reader.CanRead)
            {
                Lexer.Expect(PdfObjectTokenKind.EndDictionary);
            }
        }
        else
        {
            Lexer.Expect(PdfObjectTokenKind.EndDictionary);
        }

        // Is there a stream as well?
        var stream = ParseStream(result);
        if (stream != null)
        {
            return stream;
        }

        return result;
    }

    private PdfArray ParseArray()
    {
        Lexer.Expect(PdfObjectTokenKind.BeginArray);

        var result = new PdfArray();
        while (Lexer.Peek(out var token))
        {
            if (token.Kind == PdfObjectTokenKind.EndArray)
            {
                break;
            }

            result.Add(ParseObject());
        }

        Lexer.Expect(PdfObjectTokenKind.EndArray);

        return result;
    }

    private PdfObject? ParseStream(PdfDictionary metadata)
    {
        // Not a stream?
        if (!Lexer.Peek(out var streamToken) || streamToken.Kind != PdfObjectTokenKind.BeginStream)
        {
            return null;
        }

        var length = metadata.ReadOptionalInteger(PdfName.Known.Length);
        if (length == null)
        {
            throw new InvalidOperationException("Stream did not have a specified length");
        }

        // Read the stream data
        Lexer.Expect(PdfObjectTokenKind.BeginStream);
        Lexer.EatNewlines();
        var data = Lexer.ReadBytes(length.Value);
        Lexer.Expect(PdfObjectTokenKind.EndStream);

        var stream = new PdfStream(metadata, data.ToArray());

        return stream;
    }

    private PdfObject ParseXRefTable()
    {
        Lexer.Expect(PdfObjectTokenKind.XRef);

        var table = new PdfXRefTable();

        while (Lexer.Reader.CanRead)
        {
            if (!Lexer.Check(PdfObjectTokenKind.Integer))
            {
                break;
            }

            var startId = Lexer.Expect(PdfObjectTokenKind.Integer).ParseInteger();
            var count = Lexer.Expect(PdfObjectTokenKind.Integer).ParseInteger();

            foreach (var id in Enumerable.Range(startId, count))
            {
                var position = Lexer.Expect(PdfObjectTokenKind.Integer).ParseInteger();
                var generation = Lexer.Expect(PdfObjectTokenKind.Integer).ParseInteger();

                switch (Lexer.Read().Kind)
                {
                    case PdfObjectTokenKind.XRefFree:
                        table.Add(new PdfFreeXRef(new PdfObjectId(id, generation)));
                        break;
                    case PdfObjectTokenKind.XRefIndirect:
                        table.Add(new PdfIndirectXRef(
                            new PdfObjectId(id, generation),
                            position));
                        break;
                    default:
                        throw new InvalidOperationException("Unknown xref kind encountered");
                }
            }
        }

        return table;
    }
}