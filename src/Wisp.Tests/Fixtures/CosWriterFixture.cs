using System.Text;

namespace Wisp.Testing.Fixtures;

public sealed class CosWriterFixture
{
    private readonly MemoryStream _stream;
    private readonly CosWriter _writer;
    private readonly CosDocument _owner;

    public StringResult Result => new StringResult(Encoding.ASCII.GetString(_stream.ToArray()));

    public BytesResult RawResult => new BytesResult(_stream.ToArray());

    public CosWriterFixture(CosWriterSettings? settings = null)
    {
        _stream = new MemoryStream();
        _writer = new CosWriter(_stream, settings ?? new());
        _owner = new CosDocument();
    }

    public void Write(ICosPrimitive primitive)
    {
        _writer.Write(_owner, primitive);
    }
}