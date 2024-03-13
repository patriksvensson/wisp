namespace Wisp.Internal;

internal static class EnumerableExtensions
{
    public static IEnumerable<(int Index, bool First, bool Last, T Item)> Enumerate<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return Enumerate(source.GetEnumerator());
    }

    private static IEnumerable<(int Index, bool First, bool Last, T Item)> Enumerate<T>(this IEnumerator<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var first = true;
        var last = !source.MoveNext();

        for (var index = 0; !last; index++)
        {
            var current = source.Current;
            last = !source.MoveNext();
            yield return (index, first, last, current);
            first = false;
        }
    }

    public static IEnumerable<T> Flatten<T>(this IEnumerable<T[]> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.SelectMany(x => x);
    }

    // https://stackoverflow.com/a/48388133/936
    public static IEnumerable<int[]> GroupConsecutive(
        this IEnumerable<int> iterable,
        Func<int, int>? ordering = null)
    {
        return GroupConsecutive<int>(iterable, n => n);
    }

    // https://stackoverflow.com/a/48388133/936
    public static IEnumerable<T[]> GroupConsecutive<T>(
        this IEnumerable<T> iterable,
        Func<T, int> getter)
    {
        foreach (var tg in iterable
                     .Select((e, i) => (e, i))
                     .GroupBy(t => t.i - getter(t.e)))
        {
            yield return tg.Select(t => t.e).ToArray();
        }
    }
}