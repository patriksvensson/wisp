namespace Wisp;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosDate : ICosPrimitive
{
    private static readonly string[] _formats =
    [
        "yyyyMMddHHmmsszzz",
        "yyyyMMddHHmmsszz",
        "yyyyMMddHHmmss",
        "yyyyMMddHHmm",
        "yyyyMMddHH",
        "yyyyMMdd",
        "yyyyMM",
        "yyyy"
    ];

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

    [DebuggerStepThrough]
    public void Accept<TContext>(ICosVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitDate(this, context);
    }

    [DebuggerStepThrough]
    public TResult Accept<TContext, TResult>(ICosVisitor<TContext, TResult> visitor, TContext context)
    {
        return visitor.VisitDate(this, context);
    }

    public override string ToString()
    {
        return "[ObjectID] " +
               Value.ToString(
                   "yyyyMMddHHmmsszzz",
                   CultureInfo.InvariantCulture);
    }
}