namespace Wisp.Testing;

public static class EmbeddedResourceReader
{
    public static string Read(string resourceName)
    {
        if (resourceName is null)
        {
            throw new ArgumentNullException(nameof(resourceName));
        }

        var frame = new StackFrame(1);
        var assembly = frame.GetMethod()?.DeclaringType?.Assembly;
        if (assembly == null)
        {
            throw new InvalidOperationException("Could not resolve caller.");
        }

        resourceName = resourceName.Replace("/", ".", StringComparison.Ordinal);
        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                throw new InvalidOperationException("Could not load manifest resource stream.");
            }

            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }

    public static Stream? GetStream(string resourceName)
    {
        if (resourceName is null)
        {
            throw new ArgumentNullException(nameof(resourceName));
        }

        var frame = new StackFrame(1);
        var assembly = frame.GetMethod()?.DeclaringType?.Assembly;
        if (assembly == null)
        {
            throw new InvalidOperationException("Could not resolve caller.");
        }

        resourceName = resourceName.Replace("/", ".", StringComparison.Ordinal);
        return assembly.GetManifestResourceStream(resourceName);
    }
}