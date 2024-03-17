using System.ComponentModel;
using System.Reflection.Metadata;

namespace Wisp.Cli;

[UsedImplicitly]
public sealed class UpdateCommand : Command<UpdateCommand.Setting>
{
    private static string[] _compressions = ["none", "fastest", "optimal", "smallest"];

    [UsedImplicitly]
    public sealed class Setting : CommandSettings
    {
        [CommandArgument(0, "<INPUT>")]
        public string Input { get; set; } = null!;

        [CommandOption("-o|--out <OUTPUT>")]
        [Description("The output file")]
        public string Output { get; set; } = null!;

        [CommandOption("--unpack")]
        [Description("Unpacks object streams during write")]
        public bool Unpack { get; set; }

        [CommandOption("--compression")]
        [Description("The compression to use: [blue]none[/], [blue]fastest[/], [blue]optimal[/], [blue]smallest[/]")]
        [DefaultValue("optimal")]
        public string Compression { get; set; } = null!;

        public override ValidationResult Validate()
        {
            if (Output == null)
            {
                return ValidationResult.Error("The option [blue]--out[/] has not been set");
            }

            if (!_compressions.Contains(Compression))
            {
                return ValidationResult.Error($"Unknown compression [blue]{Compression}[/]");
            }

            return base.Validate();
        }
    }

    public override int Execute(CommandContext context, Setting settings)
    {
        var document = CosDocument.Open(File.OpenRead(settings.Input));

        document.Save(
            File.OpenWrite(settings.Output),
            compression: GetCompression(settings.Compression),
            unpack: settings.Unpack);

        AnsiConsole.MarkupLine("✅ [green]Done![/]");
        return 0;
    }

    private static CosCompression GetCompression(string compression)
    {
        return compression switch
        {
            "none" => CosCompression.None,
            "fastest" => CosCompression.Fastest,
            "optimal" => CosCompression.Optimal,
            "smallest" => CosCompression.Smallest,
            _ => throw new InvalidOperationException("Unknown compression"),
        };
    }
}