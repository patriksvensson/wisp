namespace Wisp.Cos;

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
        get => _dictionary[key];
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

public static class PdfDictionaryExtensions
{
    public static CosPrimitive? Get(this CosDictionary dictionary, CosName key)
    {
        if (dictionary is null)
        {
            throw new System.ArgumentNullException(nameof(dictionary));
        }

        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        dictionary.TryGetValue(key, out var value);
        return value;
    }

    public static void SetIfNotNull(this CosDictionary dictionary, CosName key, CosPrimitive? value)
    {
        if (value != null)
        {
            dictionary.Set(key, value);
        }
    }

    public static T? GetOptionalValue<T>(this CosDictionary dictionary, CosName key)
        where T : CosPrimitive
    {
        if (!dictionary.TryGetValue(key, out var obj))
        {
            return default;
        }

        if (obj is not T item)
        {
            throw new InvalidOperationException(
                $"Expected key '{key.Value}' to be of type '{typeof(T).Name}', " +
                $"but it was of type '{obj.GetType().Name}'");
        }

        return item;
    }

    public static T GetOptionalValue<T>(this CosDictionary dictionary, CosName key, Func<T> func)
        where T : CosPrimitive
    {
        if (!dictionary.TryGetValue(key, out var obj))
        {
            return func();
        }

        if (obj is not T item)
        {
            throw new InvalidOperationException(
                $"Expected key '{key.Value}' to be of type '{typeof(T).Name}', " +
                $"but it was of type '{obj.GetType().Name}'");
        }

        return item;
    }

    public static T GetRequiredValue<T>(this CosDictionary dictionary, CosName key)
        where T : CosPrimitive
    {
        var result = GetOptionalValue<T>(dictionary, key);
        if (result == null)
        {
            throw new InvalidOperationException($"Could not get value for required key '{key.Value}'.");
        }

        return result;
    }

    public static int ReadRequiredInteger(this CosDictionary dictionary, CosName key)
    {
        return GetRequiredValue<CosInteger>(dictionary, key).Value;
    }

    public static int? ReadOptionalInteger(this CosDictionary dictionary, CosName key)
    {
        return GetOptionalValue<CosInteger>(dictionary, key)?.Value;
    }
}