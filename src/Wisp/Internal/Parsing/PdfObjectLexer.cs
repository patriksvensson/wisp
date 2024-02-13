namespace Wisp.Internal;

internal sealed class PdfObjectLexer : IDisposable
{
    public IByteReader Reader { get; }

    public PdfObjectLexer(Stream stream)
    {
        Reader = new ByteReader(stream);
    }

    public PdfObjectLexer(IByteReader reader)
    {
        Reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    public void Dispose()
    {
        Reader.Dispose();
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

    public bool Peek([NotNullWhen(true)] out PdfObjectToken? token)
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

    public bool Check(PdfObjectTokenKind kind)
    {
        if (Peek(out var token))
        {
            return token.Kind == kind;
        }

        return false;
    }

    public PdfObjectToken Expect(PdfObjectTokenKind kind)
    {
        var token = Read();
        if (token.Kind != kind)
        {
            throw new InvalidOperationException(
                $"Expected '{kind}' token in stream but found '{token.Kind}'");
        }

        return token;
    }

    public PdfObjectToken Read()
    {
        if (!TryRead(out var token))
        {
            throw new InvalidOperationException("Could not read next token from stream");
        }

        return token;
    }

    public bool TryRead([NotNullWhen(true)] out PdfObjectToken? token)
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

    private PdfObjectToken ReadComment()
    {
        Reader.Discard('%');

        while (Reader.CanRead)
        {
            var current = Reader.PeekChar();
            if (current.IsPdfLineBreak())
            {
                break;
            }

            Reader.ReadByte();
        }

        return new PdfObjectToken(
            PdfObjectTokenKind.Comment);
    }

    private PdfObjectToken ReadName()
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

        return new PdfObjectToken(
            PdfObjectTokenKind.Name,
            accumulator.ToString());
    }

    private PdfObjectToken ReadStringLiteral()
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

        return new PdfObjectToken(
            PdfObjectTokenKind.StringLiteral,
            text: null,
            lexeme: accumulator.ToArray());
    }

    private PdfObjectToken ReadBeginDictionaryOrHexStringLiteral()
    {
        Reader.Discard('<');

        if (Reader.PeekChar() == '<')
        {
            Reader.Discard('<');
            return new PdfObjectToken(PdfObjectTokenKind.BeginDictionary);
        }

        return ReadHexStringLiteral();
    }

    private PdfObjectToken ReadHexStringLiteral()
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

        return new PdfObjectToken(
            PdfObjectTokenKind.HexStringLiteral,
            HexUtility.FromHex(accumulator.ToString()));
    }

    private PdfObjectToken ReadBeginArray()
    {
        Reader.Discard('[');
        return new PdfObjectToken(PdfObjectTokenKind.BeginArray);
    }

    private PdfObjectToken ReadEndArray()
    {
        Reader.Discard(']');
        return new PdfObjectToken(PdfObjectTokenKind.EndArray);
    }

    private PdfObjectToken ReadEndDictionary()
    {
        Reader.Discard('>');
        Reader.Discard('>');
        return new PdfObjectToken(PdfObjectTokenKind.EndDictionary);
    }

    private PdfObjectToken ReadNumber()
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

        return new PdfObjectToken(
            encounteredPeriod ? PdfObjectTokenKind.Real : PdfObjectTokenKind.Integer,
            number);
    }

    private PdfObjectToken ReadKeyword()
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
                return new PdfObjectToken(PdfObjectTokenKind.Boolean, "true");
            case "false":
                return new PdfObjectToken(PdfObjectTokenKind.Boolean, "false");
            case "trailer":
                return new PdfObjectToken(PdfObjectTokenKind.Trailer);
            case "obj":
                return new PdfObjectToken(PdfObjectTokenKind.BeginObject);
            case "endobj":
                return new PdfObjectToken(PdfObjectTokenKind.EndObject);
            case "stream":
                return new PdfObjectToken(PdfObjectTokenKind.BeginStream);
            case "endstream":
                return new PdfObjectToken(PdfObjectTokenKind.EndStream);
            case "null":
                return new PdfObjectToken(PdfObjectTokenKind.Null);
            case "R":
                return new PdfObjectToken(PdfObjectTokenKind.Reference);
            case "startxref":
                return new PdfObjectToken(PdfObjectTokenKind.StartXRef);
            case "xref":
                return new PdfObjectToken(PdfObjectTokenKind.XRef);
            case "f":
                return new PdfObjectToken(PdfObjectTokenKind.XRefFree);
            case "n":
                return new PdfObjectToken(PdfObjectTokenKind.XRefIndirect);
            default:
                throw new InvalidOperationException($"Unknown token '{keyword}'");
        }
    }
}