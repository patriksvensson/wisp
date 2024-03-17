namespace Wisp.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandApp<UpdateCommand>();
        app.Configure(config =>
        {
            config.AddCommand<ReadCommand>("read").IsHidden();

            config.SetApplicationName("wisp");
            config.SetExceptionHandler(ex =>
            {
                if (ex is CommandRuntimeException)
                {
                    AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
                }
                else
                {
                    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
                }
            });
        });

        return app.Run(args);
    }
}