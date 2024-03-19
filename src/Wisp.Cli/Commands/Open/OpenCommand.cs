namespace Wisp.Cli;

public abstract class OpenCommand<T> : Command<T>
    where T : OpenSettings
{
    public sealed override int Execute(CommandContext context, T settings)
    {
        using var stream = File.OpenRead(settings.Input);
        var document = CosDocument.Open(stream);
        Execute(context, settings, document);
        return 0;
    }

    protected abstract void Execute(CommandContext context, T settings, CosDocument document);
}

public abstract class OpenSettings : CommandSettings
{
    [CommandArgument(0, "<INPUT>")]
    public string Input { get; }

    protected OpenSettings(string input)
    {
        Input = input ?? throw new ArgumentNullException(nameof(input));
    }
}