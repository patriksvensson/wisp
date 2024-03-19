using Wisp.Cli.Show;

namespace Wisp.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandApp();
        app.Configure(config =>
        {
            config.AddBranch<OpenSettings>("open", open =>
            {
                open.AddCommand<SaveCommand>("save")
                    .WithDescription("Saves the PDF file");

                open.AddBranch<ShowSettings>("show", show =>
                {
                    show.SetDescription("Show different aspects of the PDF file");

                    show.AddCommand<ShowXRefTableCommand>("xref")
                        .WithDescription("Shows the PDF file's cross reference (xref) table");

                    show.AddCommand<ShowObjectsCommand>("objects")
                        .WithDescription("Shows all objects in the PDF file");
                });
            });

            config.SetApplicationName("dotnet wisp");
            config.AddExample("open", "MyFile.pdf", "show", "xref");
            config.AddExample("open", "MyFile.pdf", "save", "MyNewFile.pdf", "--unpack");
            config.ValidateExamples();
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