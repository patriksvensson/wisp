namespace Wisp.Filters;

[PublicAPI]
public sealed class FilterPipeline
{
    private readonly List<Filter> _pipeline;

    private static readonly Dictionary<string, Filter> _filters = new(StringComparer.Ordinal)
    {
        { "ASCIIHexDecode", new AsciiHexFilter() },
        { "ASCII85Decode", new Ascii85Filter() },
        { "LZWDecode", new LzwFilter() },
        { "FlateDecode", new FlateFilter() },
        { "RunLengthDecode", new RunLengthFilter() },
        { "CCITTFaxDecode", new CcittFaxFilter() },
        { "JBIG2Decode", new Jbig2Filter() },
        { "DCTDecode", new DctFilter() },
        { "JPXDecode", new JpxFilter() },
        { "Crypt", new CryptFilter() },
    };

    private FilterPipeline()
    {
        _pipeline = new List<Filter>();
    }

    public FilterPipeline(Filter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        _pipeline = new List<Filter>(new[] { filter });
    }

    public FilterPipeline(IEnumerable<Filter>? filters)
    {
        _pipeline = new List<Filter>(filters ?? Enumerable.Empty<Filter>());
    }

    public static FilterPipeline Create(CosDictionary dictionary)
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        // Get the parameters
        var decodeParams = dictionary.GetOptional(
            CosName.Known.DecodeParms,
            () => new CosDictionary());

        // Is there a filter?
        if (dictionary.TryGetValue(CosName.Known.Filter, out var filterObj))
        {
            return filterObj switch
            {
                CosArray filterArray => CreateFilterPipeline(filterArray, decodeParams),
                CosName filterName => new FilterPipeline(CreateFilter(filterName, decodeParams)),
                _ => throw new InvalidOperationException("Invalid filter primitive"),
            };
        }

        return new FilterPipeline();
    }

    public byte[] Decode(byte[] data, CosDictionary parameters)
    {
        // Any unsupported filters?
        if (_pipeline.Any(x => !x.Supported))
        {
            var unsupported = _pipeline.Where(x => !x.Supported).ToArray();
            if (unsupported.Length == 1)
            {
                throw new InvalidOperationException(
                    $"Cannot decode data. The filter {unsupported[0]} is not supported");
            }

            var unsupportedNames = string.Join(",", unsupported.Select(x => x.Name));
            throw new InvalidOperationException(
                $"Cannot decode data. The following filters are unsupported: {unsupportedNames}");
        }

        foreach (var filter in _pipeline)
        {
            data = filter.Decode(data, parameters);
        }

        return data;
    }

    private static FilterPipeline CreateFilterPipeline(CosArray filters, CosDictionary parameters)
    {
        var result = new List<Filter>();

        foreach (var filter in filters)
        {
            if (filter is not CosName filterName)
            {
                throw new InvalidOperationException("Expected filter name to be a COS name");
            }

            result.Add(CreateFilter(filterName, parameters));
        }

        return new FilterPipeline(result);
    }

    private static Filter CreateFilter(CosName filterName, CosDictionary parameters)
    {
        if (_filters.TryGetValue(filterName.Value, out var filter))
        {
            return filter;
        }

        throw new NotSupportedException($"Unsupported filter '{filterName.Value}'");
    }
}