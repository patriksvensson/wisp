namespace Wisp.Cli;

[UsedImplicitly]
public class ReadCommand : Command<ReadCommand.Settings>
{
    [UsedImplicitly]
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<INPUT>")]
        public string Input { get; }

        public Settings(string input)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
        }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        using var document = CosDocument.Open(File.OpenRead(settings.Input));
        AnsiConsole.MarkupLine("✅ [green]Document was read successfully[/]");
        return 0;
    }
}