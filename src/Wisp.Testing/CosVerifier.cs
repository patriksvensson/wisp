namespace Wisp.Testing;

public static class CosVerifier
{
    public static SettingsTask Verify(
        CosDocument model,
        CosSerializerSettings? settings = null)
    {
        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        settings ??= new CosSerializerSettings();
        var output = CosSerializer.Serialize(model, settings);
        return Verifier.Verify(output, extension: "xml");
    }
}