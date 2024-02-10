namespace Wisp;

public sealed class Parser
{
    private readonly Lexer _lexer;

    public Parser(IBufferReader reader)
    {
        _lexer = new Lexer(reader);
    }

    public Parser(Lexer lexer)
    {
        _lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
    }

    public PdfObject ReadObject()
    {
        while (_lexer.Check(TokenKind.Comment))
        {
            _lexer.Read();
        }

        if (!_lexer.Peek(out var token))
        {
            throw new InvalidOperationException("Reached end of buffer");
        }

        return token.Kind switch
        {
            TokenKind.Null => ReadNull(),
            TokenKind.Boolean => ReadBoolean(),
            TokenKind.Integer => ReadInteger(),
            TokenKind.Real => ReadReal(),
            TokenKind.StringLiteral => ReadStringLiteral(),
            TokenKind.HexStringLiteral => ReadHexStringLiteral(),
            TokenKind.Name => ReadName(),
            TokenKind.BeginDictionary => ReadDictionary(),
            TokenKind.BeginArray => ReadArray(),
            _ => throw new InvalidOperationException($"Unknown token {token.Kind} encountered in stream"),
        };
    }

    private PdfObject ReadBoolean()
    {
        var token = _lexer.Expect(TokenKind.Boolean);
        return (token.Text == "true")
            ? new PdfBoolean(true)
            : new PdfBoolean(false);
    }

    private PdfObject ReadInteger()
    {
        var value = _lexer.Expect(TokenKind.Integer).ParseInteger();
        var position = _lexer.Reader.Position;

        // Got an integer next?
        if (_lexer.Peek(out var token) && token.Kind == TokenKind.Integer)
        {
            var generation = _lexer.Expect(TokenKind.Integer).ParseInteger();

            if (_lexer.Peek(out token))
            {
                if (token.Kind == TokenKind.Reference)
                {
                    // Reference means object ID
                    _lexer.Expect(TokenKind.Reference);
                    return new PdfObjectId(value, generation);
                }
                else if (token.Kind == TokenKind.BeginObject)
                {
                    // Object definition
                    _lexer.Expect(TokenKind.BeginObject);
                    return new PdfObjectDefinition(
                        new PdfObjectId(value, generation),
                        ReadObject());
                }
            }

            // Rewind the reader
            _lexer.Reader.Seek(position, SeekOrigin.Begin);
        }

        return new PdfInteger(value);
    }

    private PdfObject ReadReal()
    {
        var value = _lexer.Expect(TokenKind.Real).ParseReal();
        return new PdfReal(value);
    }

    private PdfObject ReadNull()
    {
        _lexer.Expect(TokenKind.Null);
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

        var token = _lexer.Expect(TokenKind.StringLiteral);
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
        var token = _lexer.Expect(TokenKind.HexStringLiteral);
        return new PdfString(token.Text!, PdfStringEncoding.HexLiteral);
    }

    private PdfObject ReadName()
    {
        var token = _lexer.Expect(TokenKind.Name);
        return new PdfName(token.Text!);
    }

    private PdfObject ReadDictionary()
    {
        _lexer.Expect(TokenKind.BeginDictionary);

        var result = new PdfDictionary();
        while (_lexer.Peek(out var token))
        {
            if (token.Kind == TokenKind.EndDictionary)
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

        _lexer.Expect(TokenKind.EndDictionary);

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
        _lexer.Expect(TokenKind.BeginArray);

        var result = new PdfArray();
        while (_lexer.Peek(out var token))
        {
            if (token.Kind == TokenKind.EndArray)
            {
                break;
            }

            result.Add(ReadObject());
        }

        _lexer.Expect(TokenKind.EndArray);

        return result;
    }

    private PdfObject? ParseStream(PdfDictionary metadata)
    {
        // Not a stream?
        if (!_lexer.Peek(out var streamToken) || streamToken.Kind != TokenKind.BeginStream)
        {
            return null;
        }

        var length = metadata.ReadOptionalInteger(PdfName.Known.Length);
        if (length == null)
        {
            throw new InvalidOperationException("Stream did not have a specified length");
        }

        // Read the stream data
        _lexer.Expect(TokenKind.BeginStream);
        _lexer.EatNewlines();
        var data = _lexer.ReadBytes(length.Value);
        _lexer.Expect(TokenKind.EndStream);

        var stream = new PdfStream(metadata, data.ToArray());

        return stream;
    }
}