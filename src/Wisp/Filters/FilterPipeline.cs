namespace Wisp.Filters;

[PublicAPI]
public sealed class FilterPipeline
{
    private readonly List<Filter> _filters;

    public FilterPipeline(IEnumerable<Filter>? filters)
    {
        _filters = new List<Filter>(filters ?? Enumerable.Empty<Filter>());
    }

    public byte[] Decode(byte[] data, CosDictionary? parameters)
    {
        // Any unsupported filters?
        if (_filters.Any(x => !x.Supported))
        {
            var unsupported = _filters.Where(x => !x.Supported).ToArray();
            if (unsupported.Length == 1)
            {
                throw new InvalidOperationException(
                    $"Cannot decode data. The filter {unsupported[0]} is not supported");
            }

            var unsupportedNames = string.Join(",", unsupported.Select(x => x.Name));
            throw new InvalidOperationException(
                $"Cannot decode data. The following filters are unsupported: {unsupportedNames}");
        }

        // Pass the data through each filter
        foreach (var filter in _filters)
        {
            data = filter.Decode(data, parameters);
        }

        return data;
    }

    internal static class Factory
    {
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

        public static FilterPipeline Create(CosStream stream)
        {
            // Is there a filter?
            if (stream.Metadata.TryGetValue(CosNames.Filter, out var filterObj))
            {
                return filterObj switch
                {
                    CosArray filterArray => CreateFilterPipeline(filterArray),
                    CosName filterName => new FilterPipeline(new[] { CreateFilter(filterName) }),
                    _ => throw new InvalidOperationException("Invalid filter primitive"),
                };
            }

            return new FilterPipeline(Array.Empty<Filter>());
        }

        private static FilterPipeline CreateFilterPipeline(CosArray filters)
        {
            var result = new List<Filter>();

            foreach (var filter in filters)
            {
                if (filter is not CosName filterName)
                {
                    throw new InvalidOperationException("Expected filter name to be a COS name");
                }

                result.Add(CreateFilter(filterName));
            }

            return new FilterPipeline(result);
        }

        private static Filter CreateFilter(CosName filterName)
        {
            if (_filters.TryGetValue(filterName.Value, out var filter))
            {
                return filter;
            }

            throw new NotSupportedException($"Unsupported filter '{filterName.Value}'");
        }
    }
}