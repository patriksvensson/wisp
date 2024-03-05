namespace Wisp.Cos;

[PublicAPI]
[DebuggerDisplay("{ToString(),nq}")]
public sealed class CosDictionary : CosPrimitive, IEnumerable<KeyValuePair<CosName, CosPrimitive>>
{
    private readonly Dictionary<CosName, CosPrimitive> _dictionary;

    public int Count => _dictionary.Count;

    public CosPrimitive? this[string key]
    {
        get => this[new CosName(key)];
        set => this[new CosName(key)] = value;
    }

    public CosPrimitive? this[CosName key]
    {
        get
        {
            _dictionary.TryGetValue(key, out var value);
            return value;
        }
        set
        {
            // Setting the value to null
            // removes the pair from the dictionary
            if (value == null)
            {
                _dictionary.Remove(key);
                return;
            }

            _dictionary[key] = value;
        }
    }

    public CosDictionary()
    {
        _dictionary = new Dictionary<CosName, CosPrimitive>(CosNameComparer.Shared);
    }

    public bool ContainsKey(CosName key)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        return _dictionary.ContainsKey(key);
    }

    public void Set(CosName key, CosPrimitive value)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
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

    public bool TryGetValue(CosName key, [NotNullWhen(true)] out CosPrimitive? obj)
    {
        return _dictionary.TryGetValue(key, out obj);
    }

    public IEnumerator<KeyValuePair<CosName, CosPrimitive>> GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

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

    public static CosName? GetName(this CosDictionary dictionary, CosName key)
    {
        return dictionary.Get<CosName>(key);
    }

    public static CosObjectId? GetObjectId(this CosDictionary dictionary, CosName key)
    {
        return dictionary.Get<CosObjectId>(key);
    }

    public static CosDictionary? GetDictionary(this CosDictionary dictionary, CosName key)
    {
        return dictionary.Get<CosDictionary>(key);
    }

    public static CosArray? GetArray(this CosDictionary dictionary, CosName key)
    {
        return dictionary.Get<CosArray>(key);
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
        where T : CosPrimitive
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
            return null;
#endif
        }

        return item;
    }
}