using System.Text;

namespace Wisp.Testing.Fixtures;

public sealed class CosWriterFixture
{
    private readonly MemoryStream _stream;
    private readonly CosWriter _writer;

    public StringResult Result => new StringResult(Encoding.ASCII.GetString(_stream.ToArray()));

    public CosWriterFixture(CosWriterSettings? settings = null)
    {
        _stream = new MemoryStream();
        _writer = new CosWriter(new CosDocument(), _stream, settings ?? new());
    }

    public void Write(ICosPrimitive primitive)
    {
        _writer.Write(primitive);
    }
}