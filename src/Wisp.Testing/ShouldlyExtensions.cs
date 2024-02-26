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

    public static CosBoolean ShouldHaveValue(this CosBoolean obj, bool value)
    {
        obj.ShouldNotBeNull();
        obj.Value.ShouldBe(value);
        return obj;
    }

    public static CosInteger ShouldHaveValue(this CosInteger obj, int value)
    {
        obj.ShouldNotBeNull();
        obj.Value.ShouldBe(value);
        return obj;
    }

    public static CosReal ShouldHaveValue(this CosReal obj, double value)
    {
        obj.ShouldNotBeNull();
        obj.Value.ShouldBe(value);
        return obj;
    }

    public static CosString ShouldHaveValue(this CosString obj, string value)
    {
        obj.ShouldNotBeNull();
        obj.Value.ShouldBe(value);
        return obj;
    }

    public static CosString ShouldHaveEncoding(this CosString obj, CosStringEncoding value)
    {
        obj.ShouldNotBeNull();
        obj.Encoding.ShouldBe(value);
        return obj;
    }

    public static CosName ShouldHaveValue(this CosName obj, string value)
    {
        obj.ShouldNotBeNull();
        obj.Value.ShouldBe(value);
        return obj;
    }

    public static CosObjectId ShouldHaveNumber(this CosObjectId obj, int value)
    {
        obj.ShouldNotBeNull();
        obj.Number.ShouldBe(value);
        return obj;
    }

    public static CosObjectId ShouldHaveGeneration(this CosObjectId obj, int value)
    {
        obj.ShouldNotBeNull();
        obj.Generation.ShouldBe(value);
        return obj;
    }

    public static CosDictionary ShouldHaveKeyValue(this CosDictionary obj, string key, int value)
    {
        var pdfKey = new CosName(key);
        obj.ContainsKey(pdfKey).ShouldBeTrue();
        obj[pdfKey].ShouldBeOfType<CosInteger>().ShouldHaveValue(value);
        return obj;
    }

    public static CosDictionary ShouldHaveKeyValue(this CosDictionary obj, string key, double value)
    {
        var pdfKey = new CosName(key);
        obj.ContainsKey(pdfKey).ShouldBeTrue();
        obj[pdfKey].ShouldBeOfType<CosReal>().ShouldHaveValue(value);
        return obj;
    }

    public static CosDate ShouldHaveDate(this CosDate obj, string value)
    {
        obj.ShouldNotBeNull();
        obj.Value.ShouldBe(DateTimeOffset.ParseExact(value, "yyyyMMddHHmmsszzz", null, DateTimeStyles.None));
        return obj;
    }

    public static CosDate ShouldHaveDate(this CosDate obj, DateTimeOffset value)
    {
        obj.ShouldNotBeNull();
        obj.Value.ShouldBe(value);
        return obj;
    }
}