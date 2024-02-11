namespace Wisp;

public sealed class PdfObjectParser
{
    public PdfObjectLexer Lexer { get; }
    public IBufferReader Reader => Lexer.Reader;

    public PdfObjectParser(IBufferReader reader)
    {
        Lexer = new PdfObjectLexer(reader);
    }

    public PdfObjectParser(PdfObjectLexer lexer)
    {
        Lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
    }

    public PdfObject ReadObject()
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
            PdfObjectTokenKind.Null => ReadNull(),
            PdfObjectTokenKind.Boolean => ReadBoolean(),
            PdfObjectTokenKind.Integer => ReadInteger(),
            PdfObjectTokenKind.Real => ReadReal(),
            PdfObjectTokenKind.StringLiteral => ReadStringLiteral(),
            PdfObjectTokenKind.HexStringLiteral => ReadHexStringLiteral(),
            PdfObjectTokenKind.Name => ReadName(),
            PdfObjectTokenKind.BeginDictionary => ReadDictionary(),
            PdfObjectTokenKind.BeginArray => ReadArray(),
            PdfObjectTokenKind.XRef => ReadXRefTable(),
            _ => throw new InvalidOperationException($"Unknown token {token.Kind} encountered in stream"),
        };
    }

    private PdfObject ReadBoolean()
    {
        var token = Lexer.Expect(PdfObjectTokenKind.Boolean);
        return (token.Text == "true")
            ? new PdfBoolean(true)
            : new PdfBoolean(false);
    }

    private PdfObject ReadInteger()
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
                        ReadObject());
                }
            }

            // Rewind the reader
            Lexer.Reader.Seek(position, SeekOrigin.Begin);
        }

        return new PdfInteger(value);
    }

    private PdfObject ReadReal()
    {
        var value = Lexer.Expect(PdfObjectTokenKind.Real).ParseReal();
        return new PdfReal(value);
    }

    private PdfObject ReadNull()
    {
        Lexer.Expect(PdfObjectTokenKind.Null);
        return new PdfNull();
    }

    private PdfObject ReadStringLiteral()
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

    private PdfObject ReadHexStringLiteral()
    {
        var token = Lexer.Expect(PdfObjectTokenKind.HexStringLiteral);
        return new PdfString(token.Text!, PdfStringEncoding.HexLiteral);
    }

    private PdfObject ReadName()
    {
        var token = Lexer.Expect(PdfObjectTokenKind.Name);
        return new PdfName(token.Text!);
    }

    private PdfObject ReadDictionary()
    {
        Lexer.Expect(PdfObjectTokenKind.BeginDictionary);

        var result = new PdfDictionary();
        while (Lexer.Peek(out var token))
        {
            if (token.Kind == PdfObjectTokenKind.EndDictionary)
            {
                break;
            }

            var temp = ReadObject();
            if (temp is not PdfName key)
            {
                throw new InvalidOperationException("Encountered dictionary key that was not a PDF name");
            }

            var value = ReadObject();
            result.Set(key, value);
        }

        Lexer.Expect(PdfObjectTokenKind.EndDictionary);

        // Is there a stream as well?
        var stream = ParseStream(result);
        if (stream != null)
        {
            return stream;
        }

        return result;
    }

    private PdfArray ReadArray()
    {
        Lexer.Expect(PdfObjectTokenKind.BeginArray);

        var result = new PdfArray();
        while (Lexer.Peek(out var token))
        {
            if (token.Kind == PdfObjectTokenKind.EndArray)
            {
                break;
            }

            result.Add(ReadObject());
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

    private PdfObject ReadXRefTable()
    {
        Lexer.Expect(PdfObjectTokenKind.XRef);

        var table = new PdfXRefTable();

        while (Reader.CanRead)
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

                var free = Lexer.Expect(PdfObjectTokenKind.Keyword).Text == "f";
                if (free)
                {
                    table.Add(new PdfFreeXRef(new PdfObjectId(id, generation)));
                }
                else
                {
                    table.Add(new PdfIndirectXRef(
                        new PdfObjectId(id, generation),
                        position));
                }
            }
        }

        return table;
    }
}