namespace Wisp.Cos;

[PublicAPI]
public sealed class CosLexer : IDisposable
{
    internal IByteStreamReader Reader { get; }

    public CosLexer(Stream stream)
    {
        Reader = new ByteStreamReader(stream);
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

    public bool Peek([NotNullWhen(true)] out CosToken? token)
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

    public bool Check(CosTokenKind kind)
    {
        if (Peek(out var token))
        {
            return token.Kind == kind;
        }

        return false;
    }

    public CosToken Expect(CosTokenKind kind)
    {
        var token = Read();
        if (token.Kind != kind)
        {
            throw new InvalidOperationException(
                $"Expected '{kind}' token in stream but found '{token.Kind}'");
        }

        return token;
    }

    public CosToken Read()
    {
        if (!TryRead(out var token))
        {
            throw new InvalidOperationException("Could not read next token from stream");
        }

        return token;
    }

    public bool TryRead([NotNullWhen(true)] out CosToken? token)
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

    private CosToken ReadComment()
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

        return new CosToken(
            CosTokenKind.Comment);
    }

    private CosToken ReadName()
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

            // Not part of spec but...
            if (current == '<' || current == '>' || current == '/' ||
                current == '[' || current == ']')
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

        return new CosToken(
            CosTokenKind.Name,
            accumulator.ToString());
    }

    private CosToken ReadStringLiteral()
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

        return new CosToken(
            CosTokenKind.StringLiteral,
            text: null,
            lexeme: accumulator.ToArray());
    }

    private CosToken ReadBeginDictionaryOrHexStringLiteral()
    {
        Reader.Discard('<');

        if (Reader.PeekChar() == '<')
        {
            Reader.Discard('<');
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

            queue.Enqueue(Reader.ReadChar());

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
        Reader.Discard('[');
        return new CosToken(CosTokenKind.BeginArray);
    }

    private CosToken ReadEndArray()
    {
        Reader.Discard(']');
        return new CosToken(CosTokenKind.EndArray);
    }

    private CosToken ReadEndDictionary()
    {
        Reader.Discard('>');
        Reader.Discard('>');
        return new CosToken(CosTokenKind.EndDictionary);
    }

    private CosToken ReadNumber()
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

        return new CosToken(
            encounteredPeriod ? CosTokenKind.Real : CosTokenKind.Integer,
            number);
    }

    private CosToken ReadKeyword()
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
                throw new InvalidOperationException($"Unknown token '{keyword}'");
        }
    }
}