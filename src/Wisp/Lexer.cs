using System.Diagnostics.CodeAnalysis;
using System.Text;
using Wisp.Extensions;

namespace Wisp;

public sealed class Lexer
{
    private readonly IBufferReader _reader;

    public Lexer(Stream stream)
    {
        _reader = new BufferReader(stream);
    }

    public Lexer(IBufferReader reader)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    public bool Peek([NotNullWhen(true)] out Token? token)
    {
        var position = _reader.Position;

        try
        {
            return Read(out token);
        }
        finally
        {
            // Move the cursor back to where we were
            _reader.Seek(position, SeekOrigin.Begin);
        }
    }

    public bool Read([NotNullWhen(true)] out Token? token)
    {
        EatWhitespace();

        if (!_reader.CanRead)
        {
            token = null;
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
        return false;
    }

    public void EatWhitespace()
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

    private Token ReadComment()
    {
        _reader.Discard('%');

        var start = _reader.Position;
        while (_reader.CanRead)
        {
            var current = _reader.PeekChar();
            if (current.IsPdfLineBreak())
            {
                break;
            }

            _reader.ReadByte();
        }

        return new Token(
            TokenKind.Comment,
            Encoding.UTF8.GetString(
                _reader.ReadBytes(
                    start, _reader.Position - start)));
    }

    private Token ReadName()
    {
        _reader.Discard('/');

        var accumulator = new StringBuilder();
        while (_reader.CanRead)
        {
            var current = _reader.PeekChar();
            if (current.IsPdfWhitespace())
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

        return new Token(
            TokenKind.Name,
            accumulator.ToString());
    }

    private Token ReadStringLiteral()
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
        _reader.Discard('<');

        if (_reader.PeekChar() == '<')
        {
            _reader.Discard('<');
            return new Token(TokenKind.BeginDictionary);
        }

        return ReadHexStringLiteral();
    }

    private Token ReadHexStringLiteral()
    {
        var accumulator = new StringBuilder();
        while (true)
        {
            if (!_reader.CanRead)
            {
                throw new InvalidOperationException(
                    "Hex string literal is missing trailing '>'.");
            }

            var current = _reader.PeekChar();
            if (!char.IsLetter(current) && !char.IsDigit(current))
            {
                if (current == '>')
                {
                    _reader.Discard('>');
                    break;
                }

                throw new InvalidOperationException(
                    $"Malformed hexadecimal literal. Invalid character '{current}'.");
            }

            accumulator.Append(_reader.ReadChar());
        }

        return new Token(
            TokenKind.HexStringLiteral,
            HexUtility.FromHex(accumulator.ToString()));
    }

    private Token ReadBeginArray()
    {
        _reader.Discard('[');
        return new Token(TokenKind.BeginArray);
    }

    private Token ReadEndArray()
    {
        _reader.Discard(']');
        return new Token(TokenKind.EndArray);
    }

    private Token ReadEndDictionary()
    {
        _reader.Discard('>');
        _reader.Discard('>');
        return new Token(TokenKind.EndDictionary);
    }

    private Token ReadNumber()
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
                    throw new InvalidOperationException("Encountered malformed integer");
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
                    throw new InvalidOperationException("Encountered more than one period");
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

        return new Token(
            encounteredPeriod ? TokenKind.Real : TokenKind.Integer,
            number);
    }

    private Token ReadKeyword()
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