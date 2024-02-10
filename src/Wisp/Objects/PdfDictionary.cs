namespace Wisp;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PdfDictionary : PdfObject, IEnumerable<KeyValuePair<PdfName, PdfObject>>
{
    private readonly Dictionary<PdfName, PdfObject> _dictionary;

    public int Count => _dictionary.Count;

    public PdfObject? this[string key]
    {
        get => this[new PdfName(key)];
        set => this[new PdfName(key)] = value;
    }

    public PdfObject? this[PdfName key]
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

    public PdfDictionary()
    {
        _dictionary = new Dictionary<PdfName, PdfObject>(PdfNameComparer.Shared);
    }

    public bool ContainsKey(PdfName key)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        return _dictionary.ContainsKey(key);
    }

    public void Set(PdfName key, PdfObject value)
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

    public bool Remove(PdfName key)
    {
        return _dictionary.Remove(key);
    }

    public void Combine(PdfDictionary other)
    {
        foreach (var kvp in other)
        {
            if (!ContainsKey(kvp.Key))
            {
                Set(kvp.Key, kvp.Value);
            }
        }
    }

    public bool TryGetValue(PdfName key, [NotNullWhen(true)] out PdfObject? obj)
    {
        return _dictionary.TryGetValue(key, out obj);
    }

    public IEnumerator<KeyValuePair<PdfName, PdfObject>> GetEnumerator()
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
    public static PdfObject? Get(this PdfDictionary dictionary, PdfName key)
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

    public static void SetIfNotNull(this PdfDictionary dictionary, PdfName key, PdfObject? value)
    {
        if (value != null)
        {
            dictionary.Set(key, value);
        }
    }

    public static T? GetOptionalValue<T>(this PdfDictionary dictionary, PdfName key)
        where T : PdfObject
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

    public static T GetOptionalValue<T>(this PdfDictionary dictionary, PdfName key, Func<T> func)
        where T : PdfObject
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

    public static T GetRequiredValue<T>(this PdfDictionary dictionary, PdfName key)
        where T : PdfObject
    {
        var result = GetOptionalValue<T>(dictionary, key);
        if (result == null)
        {
            throw new InvalidOperationException($"Could not get value for required key '{key.Value}'.");
        }

        return result;
    }

    public static int ReadRequiredInteger(this PdfDictionary dictionary, PdfName key)
    {
        return GetRequiredValue<PdfInteger>(dictionary, key).Value;
    }

    public static int? ReadOptionalInteger(this PdfDictionary dictionary, PdfName key)
    {
        return GetOptionalValue<PdfInteger>(dictionary, key)?.Value;
    }
}