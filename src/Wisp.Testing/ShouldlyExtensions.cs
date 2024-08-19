using System.Diagnostics.CodeAnalysis;

namespace Wisp.Testing;

public static class ShouldlyExtensions
{
    public static T And<T>(this T item)
    {
        return item;
    }

    public static T And<T>(this T item, Action<T> action)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(action);

        action(item);
        return item;
    }

    public static void ShouldBe(this StringResult result, string expected)
    {
        expected = expected.Replace("\r\n", "\n");
        result.Value.ShouldBe(expected);
    }

    public static void ShouldBe(this BytesResult result, byte[] expected)
    {
        result.Value.ShouldBe(expected);
    }

    public static CosBoolean ShouldHaveValue(this CosBoolean? obj, bool value)
    {
        obj.ShouldNotBeNull();
        obj.Value.ShouldBe(value);
        return obj;
    }

    public static CosInteger ShouldHaveValue(this CosInteger? obj, int value)
    {
        obj.ShouldNotBeNull();
        obj.Value.ShouldBe(value);
        return obj;
    }

    public static CosReal ShouldHaveValue(this CosReal? obj, double value)
    {
        obj.ShouldNotBeNull();
        obj.Value.ShouldBe(value);
        return obj;
    }

    public static CosString ShouldHaveValue(this CosString? obj, string value)
    {
        obj.ShouldNotBeNull();
        obj.Value.ShouldBe(value);
        return obj;
    }

    public static CosString ShouldHaveEncoding(this CosString? obj, CosStringEncoding value)
    {
        obj.ShouldNotBeNull();
        obj.Encoding.ShouldBe(value);
        return obj;
    }

    public static CosHexString ShouldHaveHexValue(this CosHexString? obj, string value)
    {
        obj.ShouldNotBeNull();
        Convert.ToHexString(obj.Value).ShouldBe(value);
        return obj;
    }

    public static CosName ShouldHaveValue(this CosName? obj, string value)
    {
        obj.ShouldNotBeNull();
        obj.Value.ShouldBe(value);
        return obj;
    }

    public static CosObjectId ShouldBe(this CosObjectId? obj, int number, int generation)
    {
        obj.ShouldNotBeNull();
        obj.Number.ShouldBe(number);
        obj.Generation.ShouldBe(generation);
        return obj;
    }

    public static CosObjectReference ShouldBe(this CosObjectReference? obj, int number, int generation)
    {
        obj.ShouldNotBeNull();
        obj.Id.ShouldBe(number, generation);
        return obj;
    }

    public static CosObjectId ShouldHaveNumber(this CosObjectId? obj, int value)
    {
        obj.ShouldNotBeNull();
        obj.Number.ShouldBe(value);
        return obj;
    }

    public static CosObjectId ShouldHaveGeneration(this CosObjectId? obj, int value)
    {
        obj.ShouldNotBeNull();
        obj.Generation.ShouldBe(value);
        return obj;
    }

    public static CosDictionary ShouldHaveKeyValue(this CosDictionary? obj, string key, int value)
    {
        var pdfKey = new CosName(key);
        obj.ShouldNotBeNull();
        obj.ContainsKey(pdfKey).ShouldBeTrue();
        obj[pdfKey].ShouldBeOfType<CosInteger>().ShouldHaveValue(value);
        return obj;
    }

    public static CosDictionary ShouldHaveKeyValue(this CosDictionary? obj, string key, double value)
    {
        var pdfKey = new CosName(key);
        obj.ShouldNotBeNull();
        obj.ContainsKey(pdfKey).ShouldBeTrue();
        obj[pdfKey].ShouldBeOfType<CosReal>().ShouldHaveValue(value);
        return obj;
    }

    public static CosDate ShouldHaveDate(this CosDate? obj, string value)
    {
        obj.ShouldNotBeNull();
        obj.Value.ShouldBe(DateTimeOffset.ParseExact(value, "yyyyMMddHHmmsszzz", null, DateTimeStyles.None));
        return obj;
    }

    public static CosDate ShouldHaveDate(this CosDate? obj, DateTimeOffset value)
    {
        obj.ShouldNotBeNull();
        obj.Value.ShouldBe(value);
        return obj;
    }
}