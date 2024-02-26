namespace Wisp.Cos;

public sealed class CosDate : CosPrimitive
{
    private static readonly string[] _formats = new[]
    {
        "yyyyMMddHHmmsszzz",
        "yyyyMMddHHmmsszz",
        "yyyyMMddHHmmss",
        "yyyyMMddHHmm",
        "yyyyMMddHH",
        "yyyyMMdd",
        "yyyyMM",
        "yyyy",
    };

    public DateTimeOffset Value { get; }

    public CosDate(DateTimeOffset value)
    {
        Value = value;
    }

    public static bool TryParse(string input, [NotNullWhen(true)] out DateTimeOffset? time)
    {
        input = input.Replace("'", ":").TrimEnd(':');
        if (string.IsNullOrWhiteSpace(input))
        {
            time = new DateTimeOffset(1, 1, 1, 0, 0, 0, TimeSpan.Zero);
            return true;
        }

        foreach (var format in _formats)
        {
            if (DateTimeOffset.TryParseExact(input, format, null, DateTimeStyles.AssumeUniversal, out var result))
            {
                time = result;
                return true;
            }
        }

        time = null;
        return false;
    }
}