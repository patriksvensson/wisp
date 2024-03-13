namespace Wisp.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandApp();
        app.Configure(config =>
        {
            config.AddCommand<ReadCommand>("read");
            config.AddBranch("update", update =>
            {
                update.AddCommand<UpdateInfoCommand>("info");
            });

            config.SetExceptionHandler(ex =>
            {
                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            });
        });

        return app.Run(args);
    }
}