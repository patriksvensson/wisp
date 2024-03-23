using System.Text;

namespace Wisp.Testing;

internal sealed class Utf8StringWriter : StringWriter
{
    public override Encoding Encoding => Encoding.UTF8;
}