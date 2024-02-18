namespace Wisp.Testing;

public static class ShouldlyExtensions
{
    public static void And<T>(this T item, Action<T> action)
        where T : class
    {
        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        action(item);
    }

    public static PdfBoolean ShouldHaveValue(this PdfBoolean obj, bool value)
    {
        obj.ShouldNotBeNull();
        obj.Value.ShouldBe(value);
        return obj;
    }

    public static PdfInteger ShouldHaveValue(this PdfInteger obj, int value)
    {
        obj.ShouldNotBeNull();
        obj.Value.ShouldBe(value);
        return obj;
    }

    public static PdfReal ShouldHaveValue(this PdfReal obj, double value)
    {
        obj.ShouldNotBeNull();
        obj.Value.ShouldBe(value);
        return obj;
    }

    public static PdfString ShouldHaveValue(this PdfString obj, string value)
    {
        obj.ShouldNotBeNull();
        obj.Value.ShouldBe(value);
        return obj;
    }

    public static PdfString ShouldHaveEncoding(this PdfString obj, PdfStringEncoding value)
    {
        obj.ShouldNotBeNull();
        obj.Encoding.ShouldBe(value);
        return obj;
    }

    public static PdfName ShouldHaveValue(this PdfName obj, string value)
    {
        obj.ShouldNotBeNull();
        obj.Value.ShouldBe(value);
        return obj;
    }

    public static PdfObjectId ShouldHaveNumber(this PdfObjectId obj, int value)
    {
        obj.ShouldNotBeNull();
        obj.Number.ShouldBe(value);
        return obj;
    }

    public static PdfObjectId ShouldHaveGeneration(this PdfObjectId obj, int value)
    {
        obj.ShouldNotBeNull();
        obj.Generation.ShouldBe(value);
        return obj;
    }

    public static PdfDictionary ShouldHaveKeyValue(this PdfDictionary obj, string key, int value)
    {
        var pdfKey = new PdfName(key);
        obj.ContainsKey(pdfKey).ShouldBeTrue();
        obj[pdfKey].ShouldBeOfType<PdfInteger>().ShouldHaveValue(value);
        return obj;
    }

    public static PdfDictionary ShouldHaveKeyValue(this PdfDictionary obj, string key, double value)
    {
        var pdfKey = new PdfName(key);
        obj.ContainsKey(pdfKey).ShouldBeTrue();
        obj[pdfKey].ShouldBeOfType<PdfReal>().ShouldHaveValue(value);
        return obj;
    }

    public static PdfDictionary ShouldHaveArrayItem(this PdfDictionary obj, string key, double value)
    {
        var pdfKey = new PdfName(key);
        obj.ContainsKey(pdfKey).ShouldBeTrue();
        obj[pdfKey].ShouldBeOfType<PdfReal>().ShouldHaveValue(value);
        return obj;
    }

    public static PdfObjectDefinition ShouldHaveId(this PdfObjectDefinition obj, int number, int generation)
    {
        obj.Id.Number.ShouldBe(number);
        obj.Id.Generation.ShouldBe(generation);
        return obj;
    }
}