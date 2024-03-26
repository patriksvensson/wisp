namespace Wisp.Security;

[Flags]
internal enum AccessPermissions : long
{
    Print = 1 << 2,
    Modify = 1 << 3,
    Extract = 1 << 4,
    ModifyAnnotations = 1 << 5,
    FillInForm = 1 << 8,
    ExtractForAccessibility = 1 << 9,
    AssembleDocument = 1 << 10,
    PrintHiRes = 1 << 11,
}