using System.Xml;

namespace Wisp.Testing;

internal class XmlWriterEx : IDisposable
{
    private readonly XmlWriter _writer;

    protected XmlWriterEx(XmlWriter writer)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    public void Dispose()
    {
        _writer.Dispose();
    }

    public void WriteElement(string name, Action action)
    {
        _writer.WriteStartElement(name);
        action();
        _writer.WriteEndElement();
    }

    public void WritePrimitive(ICosPrimitive node, Action action)
    {
        var name = node.GetType().Name
            .Replace("Cos", string.Empty)
            .Trim();

        _writer.WriteStartElement(name);
        action();
        _writer.WriteEndElement();
    }

    public void WriteString(string? name)
    {
        _writer.WriteString(name);
    }

    public void WriteAttribute<T>(string name, T value)
    {
        _writer.WriteAttributeString(name, value?.ToString());
    }

    public void WriteComment(string text)
    {
        _writer.WriteComment(text);
    }
}