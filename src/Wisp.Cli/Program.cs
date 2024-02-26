namespace Wisp.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandApp<InspectCommand>();
        return app.Run(args);
    }
}

public sealed class InspectCommand : Command<InspectCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<FILE>")]
        public string Input { get; set; } = null!;
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        return 0;
    }
}