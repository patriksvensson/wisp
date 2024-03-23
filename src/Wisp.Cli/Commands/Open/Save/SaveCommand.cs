namespace Wisp.Cli;

[UsedImplicitly]
public sealed class SaveCommand : OpenCommand<SaveCommand.Settings>
{
    private static readonly string[] _compressions =
        ["none", "fastest", "optimal", "smallest"];

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed class Settings : OpenSettings
    {
        [CommandArgument(0, "<OUTPUT>")]
        [Description("The PDF output file")]
        public string Output { get; }

        [CommandOption("--unpack")]
        [Description("Unpacks object streams during write")]
        public bool Unpack { get; set; }

        [CommandOption("--compression")]
        [Description("The compression to use: [blue]none[/], [blue]fastest[/], [blue]optimal[/], [blue]smallest[/]")]
        [DefaultValue("optimal")]
        public string Compression { get; set; } = null!;

        public Settings(string input, string output)
            : base(input)
        {
            Output = output ?? throw new ArgumentNullException(nameof(output));
        }

        public override ValidationResult Validate()
        {
            if (!_compressions.Contains(Compression))
            {
                return ValidationResult.Error($"Unknown compression [blue]{Compression}[/]");
            }

            return base.Validate();
        }
    }

    protected override void Execute(CommandContext context, Settings settings, CosDocument document)
    {
        document.Save(
            File.OpenWrite(settings.Output),
            new CosWriterSettings
            {
                Compression = GetCompression(settings.Compression),
                UnpackObjectStreams = settings.Unpack,
            });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"Wrote [italic blue]{settings.Output}[/]");
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