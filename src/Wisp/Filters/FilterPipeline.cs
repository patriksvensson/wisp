namespace Wisp.Filters;

[PublicAPI]
public sealed class FilterPipeline
{
    private readonly List<Filter> _filters;

    private FilterPipeline()
    {
        _filters = new List<Filter>();
    }

    public FilterPipeline(Filter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        _filters = new List<Filter>(new[] { filter });
    }

    public FilterPipeline(IEnumerable<Filter>? filters)
    {
        _filters = new List<Filter>(filters ?? Enumerable.Empty<Filter>());
    }

    public byte[] Decode(byte[] data)
    {
        foreach (var filter in _filters)
        {
            data = filter.Decode(data);
        }

        return data;
    }

    public static byte[] Decode(CosStream stream, byte[] data)
    {
        return CreatePipeline(stream).Decode(data);
    }

    private static FilterPipeline CreatePipeline(CosStream stream)
    {
        // Get the parameters
        var parameters = stream.Metadata.GetOptional(
            CosName.Known.DecodeParms,
            () => new CosDictionary());

        // Is there a filter?
        if (stream.Metadata.TryGetValue(CosName.Known.Filter, out var filterObj))
        {
            return filterObj switch
            {
                CosArray filterArray => CreateFilterPipeline(filterArray, parameters),
                CosName filterName => new FilterPipeline(CreateFilter(filterName, parameters)),
                _ => throw new InvalidOperationException("Could not parse filters"),
            };
        }

        return new FilterPipeline();
    }

    private static FilterPipeline CreateFilterPipeline(CosArray filterArray, CosDictionary parameters)
    {
        var filters = new List<Filter>();
        foreach (var filter in filterArray)
        {
            if (filter is not CosName filterName)
            {
                throw new InvalidOperationException("Expected filter name to be a COS name");
            }

            filters.Add(CreateFilter(filterName, parameters));
        }

        return new FilterPipeline(filters);
    }

    private static Filter CreateFilter(CosName filterName, CosDictionary parameters)
    {
        if (filterName.Value.Equals("FlateDecode", StringComparison.Ordinal))
        {
            return new DeflateFilter(parameters);
        }

        throw new NotSupportedException($"Unsupported filter '{filterName.Value}'");
    }
}