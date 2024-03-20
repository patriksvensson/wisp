namespace Wisp;

public sealed class CosLexer : IDisposable
{
    private readonly IByteStreamReader _reader;
    private bool _disposed;

    public long Position => _reader.Position;
    public long Length => _reader.Length;
    public bool CanRead => _reader.CanRead;

    public CosLexer(Stream stream)
    {
        _reader = new ByteStreamReader(stream);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _reader.Dispose();
            _disposed = true;
        }
    }

    public long Seek(long offset, SeekOrigin origin)
    {
        return _reader.Seek(offset, origin);
    }

    public int ReadByte()
    {
        return _reader.ReadByte();
    }

    public ReadOnlySpan<byte> ReadBytes(int length)
    {
        EnsureNotDisposed();

        return _reader.ReadBytes(length);
    }

    public bool Peek([NotNullWhen(true)] out CosToken? token)
    {
        EnsureNotDisposed();

        var position = _reader.Position;

        try
        {
            return TryRead(out token, out _);
        }
        finally
        {
            // Move the cursor back to where we were
            _reader.Seek(position, SeekOrigin.Begin);
        }
    }

    public bool Check(CosTokenKind kind)
    {
        EnsureNotDisposed();

        if (Peek(out var token))
        {
            return token.Kind == kind;
        }

        return false;
    }

    public CosToken Expect(CosTokenKind kind)
    {
        EnsureNotDisposed();

        try
        {
            var token = Read();
            if (token.Kind != kind)
            {
                throw new WispLexerException(
                    this, $"Expected '{kind}' token in stream but found '{token.Kind}'");
            }

            return token;
        }
        catch (Exception ex)
        {
            throw new WispLexerException(
                this,
                $"Expected token '{kind}', but lexer returned an error",
                ex);
        }
    }

    public CosToken Read()
    {
        EnsureNotDisposed();

        if (!TryRead(out var token, out var error))
        {
            throw new WispLexerException(this, $"Could not read next token from stream. Reason: {error}");
        }

        return token;
    }

    private bool TryRead([NotNullWhen(true)] out CosToken? token, [NotNullWhen(false)] out string? error)
    {
        error = null;

        EnsureNotDisposed();
        EatWhitespace();

        if (!_reader.CanRead)
        {
            token = null;
            error = "Reached end of stream";
            return false;
        }

        var current = _reader.PeekChar();

        if (current == '%')
        {
            token = ReadComment();
            return true;
        }
        else if (current == '/')
        {
            token = ReadName();
            return true;
        }
        else if (current == '(')
        {
            token = ReadStringLiteral();
            return true;
        }
        else if (current == '<')
        {
            token = ReadBeginDictionaryOrHexStringLiteral();
            return true;
        }
        else if (current == '>')
        {
            token = ReadEndDictionary();
            return true;
        }
        else if (current == '[')
        {
            token = ReadBeginArray();
            return true;
        }
        else if (current == ']')
        {
            token = ReadEndArray();
            return true;
        }
        else if (char.IsDigit(current) || current == '-' || current == '+' || current == '.')
        {
            token = ReadNumber();
            return true;
        }
        else if (char.IsLetter(current))
        {
            token = ReadKeyword();
            return true;
        }

        token = null;
        error = $"Encountered invalid token '{current}' ({Uri.HexEscape(current)})";
        return false;
    }

    private void EatWhitespace()
    {
        while (_reader.CanRead)
        {
            var current = _reader.PeekChar();
            if (!current.IsPdfWhitespace())
            {
                return;
            }

            _reader.ReadByte();
        }
    }

    private CosToken ReadComment()
    {
        _reader.Discard('%');

        while (_reader.CanRead)
        {
            var current = _reader.PeekChar();
            if (current.IsPdfLineBreak())
            {
                break;
            }

            _reader.ReadByte();
        }

        return new CosToken(
            CosTokenKind.Comment);
    }

    private CosToken ReadName()
    {
        _reader.Discard('/');

        var accumulator = new StringBuilder();
        while (_reader.CanRead)
        {
            var current = _reader.PeekChar();
            if (!current.IsPdfName() && !current.IsPdfSolidus())
            {
                break;
            }

            // Not part of spec but...
            if (current is '<' or '>' or '/' or '[' or ']' or '(' or ')')
            {
                break;
            }

            if (current == '#')
            {
                _reader.Discard('#');

                var hex = _reader.ReadBytes(2);
                accumulator.Append(HexUtility.FromHex(
                    (char)hex[0], (char)hex[1]));
            }
            else
            {
                accumulator.Append(_reader.ReadChar());
            }
        }

        return new CosToken(
            CosTokenKind.Name,
            accumulator.ToString());
    }

    private CosToken ReadStringLiteral()
    {
        _reader.Discard('(');

        var level = 0;
        var escaped = false;
        var accumulator = new List<byte>();

        while (_reader.CanRead)
        {
            var current = _reader.ReadByte();

            // Escaped new line?
            var character = (char)current;
            if ((character == '\r' || character == '\n') && escaped)
            {
                continue;
            }

            // Escape?
            if (character == '\\' && !escaped)
            {
                escaped = true;
            }
            else
            {
                if (character == '(')
                {
                    if (!escaped)
                    {
                        level++;
                    }
                }
                else if (character == ')')
                {
                    if (!escaped)
                    {
                        if (level == 0)
                        {
                            break;
                        }

                        level--;
                    }
                }

                accumulator.Add((byte)current);
                escaped = false;
            }
        }

        return new CosToken(
            CosTokenKind.StringLiteral,
            text: null,
            lexeme: accumulator.ToArray());
    }

    private CosToken ReadBeginDictionaryOrHexStringLiteral()
    {
        _reader.Discard('<');

        if (_reader.PeekChar() == '<')
        {
            _reader.Discard('<');
            return new CosToken(CosTokenKind.BeginDictionary);
        }

        return ReadHexStringLiteral();
    }

    private CosToken ReadHexStringLiteral1()
    {
        var queue = new Queue<char>();
        var result = new List<byte>();
        while (true)
        {
            if (!_reader.CanRead)
            {
                throw new WispLexerException(
                    this, "Hex string literal is missing trailing '>'.");
            }

            var current = _reader.PeekChar();
            if (!char.IsLetter(current) && !char.IsDigit(current))
            {
                if (current == '>')
                {
                    _reader.Discard('>');
                    break;
                }

                throw new WispLexerException(
                    this, $"Malformed hexadecimal literal. Invalid character '{current}'.");
            }

            queue.Enqueue(_reader.ReadChar());

            if (queue.Count == 2)
            {
                var first = queue.Dequeue();
                var second = queue.Dequeue();
                result.Add((byte)HexUtility.FromHex(first, second));
            }
        }

        return new CosToken(
            CosTokenKind.HexStringLiteral,
            lexeme: result.ToArray());
    }

    private CosToken ReadHexStringLiteral()
    {
        var accumulator = new StringBuilder();
        while (true)
        {
            if (!_reader.CanRead)
            {
                throw new WispLexerException(
                    this, "Hex string literal is missing trailing '>'.");
            }

            var current = _reader.PeekChar();
            if (!char.IsLetter(current) && !char.IsDigit(current))
            {
                if (current == '>')
                {
                    _reader.Discard('>');
                    break;
                }

                throw new WispLexerException(
                    this, $"Malformed hexadecimal literal. Invalid character '{current}'.");
            }

            accumulator.Append(_reader.ReadChar());
        }

        if (accumulator.Length % 2 != 0)
        {
            accumulator.Append('0');
        }

        return new CosToken(
            CosTokenKind.HexStringLiteral,
            lexeme: Convert.FromHexString(accumulator.ToString()));
    }

    private CosToken ReadBeginArray()
    {
        _reader.Discard('[');
        return new CosToken(CosTokenKind.BeginArray);
    }

    private CosToken ReadEndArray()
    {
        _reader.Discard(']');
        return new CosToken(CosTokenKind.EndArray);
    }

    private CosToken ReadEndDictionary()
    {
        _reader.Discard('>');
        _reader.Discard('>');
        return new CosToken(CosTokenKind.EndDictionary);
    }

    private CosToken ReadNumber()
    {
        var accumulator = new StringBuilder();
        var encounteredPeriod = false;

        while (_reader.CanRead)
        {
            var current = _reader.PeekChar();

            if (char.IsDigit(current))
            {
                accumulator.Append(_reader.ReadChar());
            }
            else if (current == '-' || current == '+')
            {
                if (accumulator.Length > 0)
                {
                    throw new WispLexerException(
                        this, "Encountered malformed integer");
                }

                _reader.Discard();
                if (current == '-')
                {
                    accumulator.Append('-');
                }
            }
            else if (current == '.')
            {
                if (encounteredPeriod)
                {
                    throw new WispLexerException(
                        this, "Encountered more than one period");
                }

                encounteredPeriod = true;
                accumulator.Append(_reader.ReadChar());
            }
            else
            {
                break;
            }
        }

        var number = accumulator.ToString();

        if (number.StartsWith('.'))
        {
            number = "0" + number;
        }
        else if (number.StartsWith("-."))
        {
            number = "-0." + number.TrimStart('-', '.');
        }

        return new CosToken(
            encounteredPeriod ? CosTokenKind.Real : CosTokenKind.Integer,
            number);
    }

    private CosToken ReadKeyword()
    {
        var accumulator = new StringBuilder();
        while (_reader.CanRead)
        {
            var current = _reader.PeekChar();
            if (!char.IsLetter(current))
            {
                break;
            }

            accumulator.Append(_reader.ReadChar());
        }

        var keyword = accumulator.ToString();
        switch (keyword)
        {
            case "true":
                return new CosToken(CosTokenKind.Boolean, "true");
            case "false":
                return new CosToken(CosTokenKind.Boolean, "false");
            case "trailer":
                return new CosToken(CosTokenKind.Trailer);
            case "obj":
                return new CosToken(CosTokenKind.BeginObject);
            case "endobj":
                return new CosToken(CosTokenKind.EndObject);
            case "stream":
                return new CosToken(CosTokenKind.BeginStream);
            case "endstream":
                return new CosToken(CosTokenKind.EndStream);
            case "null":
                return new CosToken(CosTokenKind.Null);
            case "R":
                return new CosToken(CosTokenKind.Reference);
            case "startxref":
                return new CosToken(CosTokenKind.StartXRef);
            case "xref":
                return new CosToken(CosTokenKind.XRef);
            case "f":
                return new CosToken(CosTokenKind.XRefFree);
            case "n":
                return new CosToken(CosTokenKind.XRefIndirect);
            default:
                throw new WispLexerException(
                    this, $"Unknown token '{keyword}'");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}