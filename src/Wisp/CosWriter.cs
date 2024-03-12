namespace Wisp;

[PublicAPI]
public sealed class CosWriter : IDisposable
{
    private readonly CosDocument _document;
    private readonly Stream _stream;
    private readonly CosWriterSettings _settings;

    public CosDocument Document => _document;
    public long Position => _stream.Position;

    public CosWriter(CosDocument document, Stream stream, CosWriterSettings? settings)
    {
        _document = document;
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _settings = settings ?? new();
    }

    public void Dispose()
    {
        _stream.Flush();
        _stream.Dispose();
    }

    public void Write(byte value)
    {
        _stream.WriteByte(value);
    }

    public void Write(byte[] value)
    {
        _stream.Write(value);
    }

    public void Write(char value)
    {
        _stream.WriteByte((byte)value);
    }

    public void Write(int value)
    {
        Write(value.ToString(CultureInfo.InvariantCulture));
    }

    public void Write(long value)
    {
        Write(value.ToString(CultureInfo.InvariantCulture));
    }

    public void Write(string value)
    {
        var bytes = ByteEncoding.Shared.GetBytes(value);
        _stream.Write(bytes);
    }

    public void Write(ICosPrimitive value)
    {
        var context = new Visitor.Context(_document, this, _settings);
        value.Accept(Visitor.Shared, context);
    }

    private sealed class Visitor : CosVisitor<Visitor.Context>
    {
        public static Visitor Shared { get; } = new();

        public sealed class Context(CosDocument document, CosWriter writer, CosWriterSettings settings)
        {
            public CosDocument Document { get; } = document;
            public CosWriter Writer { get; } = writer;
            public CosWriterSettings Settings { get; set; } = settings;
        }

        public override void VisitArray(CosArray obj, Context context)
        {
            context.Writer.Write('[');

            foreach (var (_, _, last, item) in obj.Enumerate())
            {
                item.Accept(this, context);

                if (!last)
                {
                    context.Writer.Write(' ');
                }
            }

            context.Writer.Write(']');
        }

        public override void VisitBoolean(CosBoolean obj, Context context)
        {
            context.Writer.Write(obj.Value ? "true" : "false");
        }

        public override void VisitDate(CosDate obj, Context context)
        {
            var timestamp = obj.Value.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            var offset = obj.Value.ToString("zzz", CultureInfo.InvariantCulture);
            context.Writer.Write("(D:");
            context.Writer.Write(timestamp + offset.Replace(':', '\''));
            context.Writer.Write(")");
        }

        public override void VisitDictionary(CosDictionary obj, Context context)
        {
            context.Writer.Write("<<");
            context.Writer.Write('\n');

            foreach (var (key, value) in obj)
            {
                key.Accept(this, context);
                context.Writer.Write(' ');
                value.Accept(this, context);
                context.Writer.Write('\n');
            }

            context.Writer.Write(">>");
        }

        public override void VisitInteger(CosInteger obj, Context context)
        {
            context.Writer.Write(obj.Value.ToString(CultureInfo.InvariantCulture));
        }

        public override void VisitName(CosName obj, Context context)
        {
            context.Writer.Write('/');
            context.Writer.Write(obj.Value);
        }

        public override void VisitNull(CosNull obj, Context context)
        {
            context.Writer.Write("null");
        }

        public override void VisitReal(CosReal obj, Context context)
        {
            context.Writer.Write(obj.Value.ToString(CultureInfo.InvariantCulture));
        }

        public override void VisitHexString(CosHexString obj, Context context)
        {
            context.Writer.Write('<');
            context.Writer.Write(Convert.ToHexString(obj.Value));
            context.Writer.Write('>');
        }

        public override void VisitObjectId(CosObjectId obj, Context context)
        {
            context.Writer.Write(obj.Number.ToString(CultureInfo.InvariantCulture));
            context.Writer.Write(' ');
            context.Writer.Write(obj.Generation.ToString(CultureInfo.InvariantCulture));
        }

        public override void VisitObjectReference(CosObjectReference obj, Context context)
        {
            obj.Id.Accept(this, context);
            context.Writer.Write(" R");
        }

        public override void VisitString(CosString obj, Context context)
        {
            context.Writer.Write('(');
            context.Writer.Write(obj.Value);
            context.Writer.Write(')');
        }

        public override void VisitObject(CosObject obj, Context context)
        {
            obj.Id.Accept(this, context);
            context.Writer.Write(" obj\n");
            obj.Object.Accept(this, context);
            context.Writer.Write("\nendobj");
        }

        public override void VisitStream(CosStream obj, Context context)
        {
            if (context.Settings.Compression == CosCompression.None)
            {
                obj.Decompress();
            }
            else
            {
                obj.Compress(context.Settings.Compression);
            }

            obj.Dictionary.Accept(this, context);
            context.Writer.Write('\n');
            context.Writer.Write("stream\n");
            context.Writer.Write(obj.GetData());
            context.Writer.Write("\nendstream");
        }

        public override void VisitObjectStream(CosObjectStream obj, Context context)
        {
            var headerStream = new MemoryStream();
            var header = new CosWriter(context.Document, headerStream, CosWriterSettings.WithoutCompression());

            var bodyStream = new MemoryStream();
            var body = new CosWriter(context.Document, bodyStream, CosWriterSettings.WithoutCompression());

            var numbers = obj.GetObjectIds().GroupConsecutive().Flatten().ToArray();
            foreach (var (_, _, last, number) in numbers.Enumerate())
            {
                var embedded = obj.GetObject(context.Document.Objects, new CosObjectId(number, 0));
                if (embedded == null)
                {
                    throw new InvalidOperationException("Could not get object stream object during write");
                }

                // Write to the body
                var start = body.Position;
                body.Write(embedded.Object);

                if (!last)
                {
                    body.Write('\n');
                }

                // Write to the header
                header.Write(number);
                header.Write(' ');
                header.Write(start);

                if (!last)
                {
                    header.Write(' ');
                }
            }

            // Write the object dictionary
            var metadata = new CosDictionary()
            {
                { CosNames.Type, CosNames.ObjStm },
                { CosNames.N, new CosInteger(numbers.Length) },
                { CosNames.First, new CosInteger(headerStream.Length + 1) },
            };

            var buffer = new MemoryStream();
            buffer.Append(headerStream);
            buffer.WriteByte((byte)'\n');
            buffer.Append(bodyStream);

            // Write everything as a stream
            context.Writer.Write(
                new CosStream(
                    metadata,
                    buffer.ToArray()));
        }
    }
}