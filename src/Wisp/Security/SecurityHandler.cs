namespace Wisp.Security;

public sealed class SecurityHandler
{
    public static SecurityHandler Create(CosDictionary dictionary)
    {
        var info = EncryptionInfo.Create(dictionary);

        throw new NotImplementedException();
    }
}