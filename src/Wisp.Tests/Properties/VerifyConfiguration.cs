using System.Runtime.CompilerServices;

public static class VerifyConfiguration
{
    [ModuleInitializer]
    public static void Init()
    {
        Verifier.DerivePathInfo(Expectations.Initialize);
    }
}