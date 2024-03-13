namespace Wisp;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public class CosDictionary : ICosPrimitive, IEnumerable<KeyValuePair<CosName, ICosPrimitive>>
{
    private readonly Dictionary<CosName, ICosPrimitive> _dictionary;

    public int Count => _dictionary.Count;

    public ICosPrimitive? this[string key]
    {
        get => this[new CosName(key)];
        set => this[new CosName(key)] = value;
    }

    public ICosPrimitive? this[CosName key]
    {
        get => Get(key);
        set => Set(key, value);
    }

    public CosDictionary()
    {
        _dictionary = new Dictionary<CosName, ICosPrimitive>(CosNameComparer.Shared);
    }

    public CosDictionary(CosDictionary dictionary)
        : this()
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        foreach (var (key, value) in dictionary)
        {
            Set(key, value);
        }
    }

    public bool ContainsKey(CosName key)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        return _dictionary.ContainsKey(key);
    }

    public void Add(CosName key, ICosPrimitive value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        if (!_dictionary.TryAdd(key, value))
        {
            throw new InvalidOperationException(
                "Another item with the same key already exist in the dictionary");
        }
    }

    public ICosPrimitive? Get(CosName key)
    {
        _dictionary.TryGetValue(key, out var value);
        return value;
    }

    public void Set(CosName key, ICosPrimitive? value)
    {
        ArgumentNullException.ThrowIfNull(key);

        // Setting the value to null
        // removes the pair from the dictionary
        if (value == null)
        {
            _dictionary.Remove(key);
            return;
        }

        _dictionary[key] = value;
    }

    public bool Remove(CosName key)
    {
        return _dictionary.Remove(key);
    }

    public void Combine(CosDictionary other)
    {
        foreach (var kvp in other)
        {
            if (!ContainsKey(kvp.Key))
            {
                Set(kvp.Key, kvp.Value);
            }
        }
    }

    public bool TryGetValue(CosName key, [NotNullWhen(true)] out ICosPrimitive? obj)
    {
        return _dictionary.TryGetValue(key, out obj);
    }

    public IEnumerator<KeyValuePair<CosName, ICosPrimitive>> GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Accept<TContext>(ICosVisitor<TContext> visitor, TContext context)
    {
        visitor.VisitDictionary(this, context);
    }

    [DebuggerStepThrough]
    public override string ToString()
    {
        return $"[Dictionary] Count = {_dictionary.Count}";
    }
}

[PublicAPI]
public static class PdfDictionaryExtensions
{
    public static CosInteger? GetInteger(this CosDictionary dictionary, CosName key)
    {
        return dictionary.Get<CosInteger>(key);
    }

    public static CosInteger GetRequiredInteger(this CosDictionary dictionary, CosName key)
    {
        return dictionary.GetRequired<CosInteger>(key);
    }

    public static CosString? GetString(this CosDictionary dictionary, CosName key)
    {
        return dictionary.Get<CosString>(key);
    }

    public static CosString GetRequiredString(this CosDictionary dictionary, CosName key)
    {
        return dictionary.GetRequired<CosString>(key);
    }

    public static CosDate? GetDate(this CosDictionary dictionary, CosName key)
    {
        return dictionary.Get<CosDate>(key);
    }

    public static CosDate GetRequiredDate(this CosDictionary dictionary, CosName key)
    {
        return dictionary.GetRequired<CosDate>(key);
    }

    public static CosName? GetName(this CosDictionary dictionary, CosName key)
    {
        return dictionary.Get<CosName>(key);
    }

    public static CosName GetRequiredName(this CosDictionary dictionary, CosName key)
    {
        return dictionary.GetRequired<CosName>(key);
    }

    public static CosObjectReference? GetObjectReference(this CosDictionary dictionary, CosName key)
    {
        return dictionary.Get<CosObjectReference>(key);
    }

    public static CosObjectReference GetRequiredObjectReference(this CosDictionary dictionary, CosName key)
    {
        return dictionary.GetRequired<CosObjectReference>(key);
    }

    public static CosDictionary? GetDictionary(this CosDictionary dictionary, CosName key)
    {
        return dictionary.Get<CosDictionary>(key);
    }

    public static CosDictionary GetRequiredDictionary(this CosDictionary dictionary, CosName key)
    {
        return dictionary.GetRequired<CosDictionary>(key);
    }

    public static CosArray? GetArray(this CosDictionary dictionary, CosName key)
    {
        return dictionary.Get<CosArray>(key);
    }

    public static CosArray GetRequiredArray(this CosDictionary dictionary, CosName key)
    {
        return dictionary.GetRequired<CosArray>(key);
    }

    public static int? GetInt32(this CosDictionary dictionary, CosName key)
    {
        return dictionary.Get<CosInteger>(key)?.IntValue;
    }

    public static long? GetInt64(this CosDictionary dictionary, CosName key)
    {
        return dictionary.Get<CosInteger>(key)?.Value;
    }

    public static T? Get<T>(this CosDictionary dictionary, CosName key)
        where T : ICosPrimitive
    {
        if (!dictionary.TryGetValue(key, out var obj))
        {
            return default;
        }

        if (obj is not T item)
        {
#if DEBUG
            throw new InvalidOperationException(
                $"Expected key '{key.Value}' to be of type '{typeof(T).Name}', " +
                $"but it was of type '{obj.GetType().Name}'");
#else
            return default;
#endif
        }

        return item;
    }

    public static T GetRequired<T>(this CosDictionary dictionary, CosName key)
        where T : ICosPrimitive
    {
        if (!dictionary.TryGetValue(key, out var obj))
        {
            throw new InvalidOperationException($"The key /{key} does not exist in the dictionary");
        }

        if (obj is not T item)
        {
            throw new InvalidOperationException(
                $"Expected required key '{key.Value}' to be of type '{typeof(T).Name}', " +
                $"but it was of type '{obj.GetType().Name}'");
        }

        return item;
    }

    public static void SetRequired(this CosDictionary dictionary, CosName key, ICosPrimitive value)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (value == null)
        {
            throw new InvalidOperationException(
                "Cannot set required key '{key}' to null");
        }

        dictionary[key] = value;
    }
}