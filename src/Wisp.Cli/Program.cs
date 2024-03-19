using Wisp.Cli.Show;

namespace Wisp.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandApp<SaveCommand>();
        app.Configure(config =>
        {
            config.AddBranch<OpenSettings>("open", open =>
            {
                open.AddBranch<ShowSettings>("show", show =>
                {
                    show.AddCommand<ShowXRefTableCommand>("xref");
                    show.AddCommand<ShowObjectsCommand>("objects");
                });
            });

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