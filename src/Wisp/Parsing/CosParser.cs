namespace Wisp.Cos;

[PublicAPI]
public sealed class CosParser : IDisposable
{
    private readonly bool _isStreamObject;

    public CosLexer Lexer { get; }

    public long Position => Lexer.Reader.Position;
    public long Length => Lexer.Reader.Length;
    public bool CanRead => Lexer.Reader.CanRead;

    public CosParser(
        Stream stream,
        bool isStreamObject = false)
    {
        ArgumentNullException.ThrowIfNull(stream);

        Lexer = new CosLexer(stream);
        _isStreamObject = isStreamObject;
    }

    public void Dispose()
    {
        Lexer.Dispose();
    }

    public long Seek(long offset, SeekOrigin origin)
    {
        return Lexer.Reader.Seek(offset, origin);
    }

    public int ReadByte()
    {
        return Lexer.Reader.ReadByte();
    }

    public ReadOnlySpan<byte> ReadBytes(int count)
    {
        return Lexer.Reader.ReadBytes(count);
    }

    public CosToken? PeekToken()
    {
        Lexer.Peek(out var token);
        return token;
    }

    public CosToken ReadToken()
    {
        return Lexer.Read();
    }

    public ICosPrimitive Parse()
    {
        while (Lexer.Check(CosTokenKind.Comment))
        {
            Lexer.Read();
        }

        if (!Lexer.Peek(out var token))
        {
            throw new InvalidOperationException("Reached end of stream");
        }

        return token.Kind switch
        {
            CosTokenKind.Null => ParseNull(),
            CosTokenKind.Boolean => ParseBoolean(),
            CosTokenKind.Integer => ParseInteger(),
            CosTokenKind.Real => ParseReal(),
            CosTokenKind.StringLiteral => ParseStringLiteral(),
            CosTokenKind.HexStringLiteral => ParseHexStringLiteral(),
            CosTokenKind.Name => ParseName(),
            CosTokenKind.BeginDictionary => ParseDictionary(),
            CosTokenKind.BeginArray => ParseArray(),
            _ => throw new InvalidOperationException($"Unexpected token {token.Kind} encountered"),
        };
    }

    private ICosPrimitive ParseBoolean()
    {
        var token = Lexer.Expect(CosTokenKind.Boolean);
        return (token.Text == "true")
            ? new CosBoolean(true)
            : new CosBoolean(false);
    }

    private ICosPrimitive ParseInteger()
    {
        var value = Lexer.Expect(CosTokenKind.Integer).ParseInt32();
        var position = Lexer.Reader.Position;

        // Got an integer next?
        if (Lexer.Peek(out var token) && token.Kind == CosTokenKind.Integer)
        {
            var generation = Lexer.Expect(CosTokenKind.Integer).ParseInt32();

            if (Lexer.Peek(out token))
            {
                switch (token.Kind)
                {
                    case CosTokenKind.Reference:
                        // Reference means object ID
                        Lexer.Expect(CosTokenKind.Reference);
                        return new CosObjectReference(new CosObjectId(value, generation));
                    case CosTokenKind.BeginObject:
                        // Object definition
                        Lexer.Expect(CosTokenKind.BeginObject);
                        return new CosObject(
                            new CosObjectId(value, generation),
                            Parse());
                }
            }

            // Rewind the reader
            Lexer.Reader.Seek(position, SeekOrigin.Begin);
        }

        return new CosInteger(value);
    }

    private CosReal ParseReal()
    {
        var value = Lexer.Expect(CosTokenKind.Real).ParseDouble();
        return new CosReal(value);
    }

    private CosNull ParseNull()
    {
        Lexer.Expect(CosTokenKind.Null);
        return CosNull.Shared;
    }

    private ICosPrimitive ParseStringLiteral()
    {
        static bool DecodeString(
            byte[] bytes,
            [NotNullWhen(true)] out string? decoded,
            [NotNullWhen(true)] out CosStringEncoding? encoding)
        {
            switch (bytes)
            {
                // Big endian unicode?
                case [0xFE, 0xFF, ..]:
                    decoded = Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);
                    encoding = CosStringEncoding.BigEndianUnicode;
                    return true;

                // Little endian unicode?
                case [0xFF, 0xFE, ..]:
                    decoded = Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
                    encoding = CosStringEncoding.Unicode;
                    return true;
            }

            // Treat everything else as raw.
            decoded = Encoding.UTF8.GetString(bytes);
            encoding = CosStringEncoding.Raw;
            return true;
        }

        var token = Lexer.Expect(CosTokenKind.StringLiteral);
        if (token.Lexeme == null)
        {
            throw new InvalidOperationException("String literal token had no byte content");
        }

        if (!DecodeString(token.Lexeme, out var decoded, out var encoding))
        {
            throw new InvalidOperationException("Could not decode PDF string");
        }

        // Is the string really a date?
        if (decoded.StartsWith("D:"))
        {
            if (CosDate.TryParse(decoded[2..], out var date))
            {
                return new CosDate(date.Value);
            }
        }

        return new CosString(decoded, encoding.Value);
    }

    private CosHexString ParseHexStringLiteral()
    {
        var token = Lexer.Expect(CosTokenKind.HexStringLiteral);
        return new CosHexString(token.Lexeme!);
    }

    private CosName ParseName()
    {
        var token = Lexer.Expect(CosTokenKind.Name);
        return new CosName(token.Text!);
    }

    private ICosPrimitive ParseDictionary()
    {
        Lexer.Expect(CosTokenKind.BeginDictionary);

        var result = new CosDictionary();
        while (Lexer.Peek(out var token))
        {
            if (token.Kind == CosTokenKind.EndDictionary)
            {
                break;
            }

            var temp = Parse();
            if (temp is not CosName key)
            {
                throw new InvalidOperationException("Encountered dictionary key that was not a PDF name");
            }

            var value = Parse();
            result.Set(key, value);
        }

        if (_isStreamObject)
        {
            if (Lexer.Reader.CanRead)
            {
                Lexer.Expect(CosTokenKind.EndDictionary);
            }
        }
        else
        {
            Lexer.Expect(CosTokenKind.EndDictionary);
        }

        // Is there a stream as well?
        var stream = ParseStream(result);
        if (stream != null)
        {
            var type = result.GetName(CosNames.Type);
            if (type?.Equals(CosNames.ObjStm) == true)
            {
                return new CosObjectStream(stream);
            }

            return stream;
        }

        return result;
    }

    private CosArray ParseArray()
    {
        Lexer.Expect(CosTokenKind.BeginArray);

        var result = new CosArray();
        while (Lexer.Peek(out var token))
        {
            if (token.Kind == CosTokenKind.EndArray)
            {
                break;
            }

            result.Add(Parse());
        }

        Lexer.Expect(CosTokenKind.EndArray);

        return result;
    }

    private CosStream? ParseStream(CosDictionary metadata)
    {
        // Not a stream?
        if (!Lexer.Peek(out var streamToken) || streamToken.Kind != CosTokenKind.BeginStream)
        {
            return null;
        }

        var length = metadata.GetInt32(CosNames.Length);
        if (length == null)
        {
            throw new InvalidOperationException("Stream did not have a specified length");
        }

        // Read the stream data
        Lexer.Expect(CosTokenKind.BeginStream);
        Lexer.EatNewlines();
        var data = Lexer.ReadBytes(length.Value);
        Lexer.Expect(CosTokenKind.EndStream);

        return new CosStream(metadata, data.ToArray());
    }
}