namespace Wisp.Security;

internal sealed class EncryptionInfo
{
    // Filter
    public required string Filter { get; init; }

    // SubFilter
    public string? SubFilter { get; init; }

    // V
    public int Algorithm { get; init; }

    // Length
    public int Length { get; init; }

    // CF
    public Filter? DictionaryFilter { get; init; }

    // StmF
    public Filter? StreamFilter { get; init; }

    // StrF
    public Filter? StringFilter { get; init; }

    // EFF
    public Filter? EmbeddedFileStreamFilter { get; init; }

    // R
    public int Revision { get; init; }

    // O
    public required byte[] OwnerPassword { get; init; }

    // U
    public required byte[] UserPassword { get; init; }

    // P
    public required AccessPermissions Permissions { get; init; }

    // EncryptMetadata
    public bool EncryptMetadata { get; init; }

    public static EncryptionInfo Create(CosDictionary dictionary)
    {
        var filter = dictionary.GetName(CosNames.Filter)?.Value ??
                     throw new WispException("/Encrypt is missing /Filter");
        if (!filter.Equals("Standard", StringComparison.OrdinalIgnoreCase))
        {
            throw new WispException($"Unsupported encryption filter '{filter}'");
        }

        var subFilter = dictionary.GetString(CosNames.SubFilter)?.Value;
        var length = dictionary.GetInteger(CosNames.Length)?.Value;
        var encryptMetadata = dictionary.GetBoolean(CosNames.EncryptMetadata)?.Value ?? true;

        var v = dictionary.GetInteger(CosNames.V)?.Value ?? 0;
        var r = dictionary.GetInteger(CosNames.R)?.Value ?? 0;
        var o = dictionary.GetString(CosNames.O)?.Value ?? throw new WispException("/Encrypt is missing /O");
        var u = dictionary.GetString(CosNames.U)?.Value ?? throw new WispException("/Encrypt is missing /U");
        var p = dictionary.GetInteger(CosNames.P)?.Value ?? throw new WispException("/Encrypt is missing /P");
        var cf = dictionary.GetDictionary(CosNames.CF);

        // Sanity checks
        if (length != null && v != 2 && v != 3)
        {
            throw new WispException("/Length is not allowed for algorithm");
        }

        return new EncryptionInfo
        {
            Filter = filter,
            SubFilter = subFilter,
            Algorithm = (int)v,
            DictionaryFilter = null,
            EmbeddedFileStreamFilter = null,
            EncryptMetadata = encryptMetadata,
            Length = 0,
            OwnerPassword = ByteEncoding.Shared.GetBytes(o),
            UserPassword = ByteEncoding.Shared.GetBytes(u),
            Permissions = (AccessPermissions)p,
            Revision = (int)r,
        };
    }
}