namespace Wisp;

public sealed class Lexer
{
    public IBufferReader Reader { get; }

    public Lexer(Stream stream)
    {
        Reader = new BufferReader(stream);
    }

    public Lexer(IBufferReader reader)
    {
        Reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    public void EatNewlines()
    {
        while (Reader.CanRead)
        {
            var current = Reader.PeekChar();
            switch (current)
            {
                case '\r':
                case '\n':
                    Reader.Discard();
                    break;
                default:
                    return;
            }
        }
    }

    public ReadOnlySpan<byte> ReadBytes(int length)
    {
        return Reader.ReadBytes(length);
    }

    public bool Peek([NotNullWhen(true)] out Token? token)
    {
        var position = Reader.Position;

        try
        {
            return TryRead(out token);
        }
        finally
        {
            // Move the cursor back to where we were
            Reader.Seek(position, SeekOrigin.Begin);
        }
    }

    public bool Check(TokenKind kind)
    {
        if (Peek(out var token))
        {
            return token.Kind == kind;
        }

        return false;
    }

    public Token Expect(TokenKind kind)
    {
        var token = Read();
        if (token.Kind != kind)
        {
            throw new InvalidOperationException(
                $"Expected '{kind}' token in stream but found '{token.Kind}'");
        }

        return token;
    }

    public Token Read()
    {
        if (!TryRead(out var token))
        {
            throw new InvalidOperationException("Could not read next token from stream");
        }

        return token;
    }

    public bool TryRead([NotNullWhen(true)] out Token? token)
    {
        EatWhitespace();

        if (!Reader.CanRead)
        {
            token = null;
            return false;
        }

        var current = Reader.PeekChar();

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
        return false;
    }

    public void EatWhitespace()
    {
        while (Reader.CanRead)
        {
            var current = Reader.PeekChar();
            if (!current.IsPdfWhitespace())
            {
                return;
            }

            Reader.ReadByte();
        }
    }

    private Token ReadComment()
    {
        Reader.Discard('%');

        var start = Reader.Position;
        while (Reader.CanRead)
        {
            var current = Reader.PeekChar();
            if (current.IsPdfLineBreak())
            {
                break;
            }

            Reader.ReadByte();
        }

        return new Token(
            TokenKind.Comment,
            Encoding.UTF8.GetString(
                Reader.ReadBytes(
                    start, Reader.Position - start)));
    }

    private Token ReadName()
    {
        Reader.Discard('/');

        var accumulator = new StringBuilder();
        while (Reader.CanRead)
        {
            var current = Reader.PeekChar();
            if (!current.IsPdfName() && !current.IsPdfSolidus())
            {
                break;
            }

            if (current == '#')
            {
                Reader.Discard('#');

                var hex = Reader.ReadBytes(2);
                accumulator.Append(HexUtility.FromHex(
                    (char)hex[0], (char)hex[1]));
            }
            else
            {
                accumulator.Append(Reader.ReadChar());
            }
        }

        return new Token(
            TokenKind.Name,
            accumulator.ToString());
    }

    private Token ReadStringLiteral()
    {
        Reader.Discard('(');

        var level = 0;
        var escaped = false;
        var accumulator = new List<byte>();

        while (Reader.CanRead)
        {
            var current = Reader.ReadByte();

            // Escaped new line?
            var character = (char)current;
            if ((character == '\r' || character == '\n') && escaped)
            {
                continue;
            }

            // Escape?
            if (character == '\\')
            {
                escaped = true;
            }
            else
            {
                if (character == '(')
                {
                    level++;
                }
                else if (character == ')')
                {
                    if (level == 0 && !escaped)
                    {
                        break;
                    }

                    level--;
                }

                accumulator.Add((byte)current);
                escaped = false;
            }
        }

        return new Token(
            TokenKind.StringLiteral,
            text: null,
            lexeme: accumulator.ToArray());
    }

    private Token ReadBeginDictionaryOrHexStringLiteral()
    {
        Reader.Discard('<');

        if (Reader.PeekChar() == '<')
        {
            Reader.Discard('<');
            return new Token(TokenKind.BeginDictionary);
        }

        return ReadHexStringLiteral();
    }

    private Token ReadHexStringLiteral()
    {
        var accumulator = new StringBuilder();
        while (true)
        {
            if (!Reader.CanRead)
            {
                throw new InvalidOperationException(
                    "Hex string literal is missing trailing '>'.");
            }

            var current = Reader.PeekChar();
            if (!char.IsLetter(current) && !char.IsDigit(current))
            {
                if (current == '>')
                {
                    Reader.Discard('>');
                    break;
                }

                throw new InvalidOperationException(
                    $"Malformed hexadecimal literal. Invalid character '{current}'.");
            }

            accumulator.Append(Reader.ReadChar());
        }

        return new Token(
            TokenKind.HexStringLiteral,
            HexUtility.FromHex(accumulator.ToString()));
    }

    private Token ReadBeginArray()
    {
        Reader.Discard('[');
        return new Token(TokenKind.BeginArray);
    }

    private Token ReadEndArray()
    {
        Reader.Discard(']');
        return new Token(TokenKind.EndArray);
    }

    private Token ReadEndDictionary()
    {
        Reader.Discard('>');
        Reader.Discard('>');
        return new Token(TokenKind.EndDictionary);
    }

    private Token ReadNumber()
    {
        var accumulator = new StringBuilder();
        var encounteredPeriod = false;

        while (Reader.CanRead)
        {
            var current = Reader.PeekChar();

            if (char.IsDigit(current))
            {
                accumulator.Append(Reader.ReadChar());
            }
            else if (current == '-' || current == '+')
            {
                if (accumulator.Length > 0)
                {
                    throw new InvalidOperationException("Encountered malformed integer");
                }

                Reader.Discard();
                if (current == '-')
                {
                    accumulator.Append('-');
                }
            }
            else if (current == '.')
            {
                if (encounteredPeriod)
                {
                    throw new InvalidOperationException("Encountered more than one period");
                }

                encounteredPeriod = true;
                accumulator.Append(Reader.ReadChar());
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

        return new Token(
            encounteredPeriod ? TokenKind.Real : TokenKind.Integer,
            number);
    }

    private Token ReadKeyword()
    {
        var accumulator = new StringBuilder();
        while (Reader.CanRead)
        {
            var current = Reader.PeekChar();
            if (!char.IsLetter(current))
            {
                break;
            }

            accumulator.Append(Reader.ReadChar());
        }

        var keyword = accumulator.ToString();
        switch (keyword)
        {
            case "true":
                return new Token(TokenKind.Boolean, "true");
            case "false":
                return new Token(TokenKind.Boolean, "false");
            case "trailer":
                return new Token(TokenKind.Trailer);
            case "obj":
                return new Token(TokenKind.BeginObject);
            case "endobj":
                return new Token(TokenKind.EndObject);
            case "stream":
                return new Token(TokenKind.BeginStream);
            case "endstream":
                return new Token(TokenKind.EndStream);
            case "null":
                return new Token(TokenKind.Null);
            case "R":
                return new Token(TokenKind.Reference);
            case "startxref":
                return new Token(TokenKind.StartXRef);
            case "xref":
                return new Token(TokenKind.XRef);
            default:
                return new Token(TokenKind.Keyword, keyword);
        }
    }
}