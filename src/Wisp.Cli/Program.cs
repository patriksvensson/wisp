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
        var stream = File.OpenRead(settings.Input);
        var document = PdfDocument.Read(stream);
        var items = document.XRefTable.ToArray();

        AnsiConsole.AlternateScreen(() =>
        {
            AnsiConsole.Clear();

            while (true)
            {
                AnsiConsole.MarkupLine($"Viewing [blue]{settings.Input}[/]");
                var xref = AnsiConsole.Prompt(new SelectionPrompt<PdfXRef>()
                    .AddChoices(items)
                    .PageSize(20)
                    .MoreChoicesText("[blue](Move up and down to reveal more choices)[/]")
                    .Title("[b][yellow]Select xref object[/][/]")
                    .WrapAround()
                    .UseConverter(GetMarkupForXRef));

                if (document.TryReadObject(xref, out var obj))
                {
                    AnsiConsole.Write(
                        PdfObjectTreeifier.ToTree(
                            GetMarkupForXRef(xref),
                            obj));
                }

                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[blue]Press ANY key to inspect again[/]");
                AnsiConsole.MarkupLine("Press [green]ESC[/] to quit");

                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape)
                {
                    break;
                }

                AnsiConsole.Clear();
            }
        });

        return 0;
    }

    private static string GetMarkupForXRef(PdfXRef xref)
    {
        if (xref is PdfIndirectXRef indirect)
        {
            return $"{xref.Id.Number}:{xref.Id.Generation} @ [yellow]{indirect.Position}[/]";
        }
        else if (xref is PdfFreeXRef)
        {
            return $"{xref.Id.Number}:{xref.Id.Generation} [blue](free)[/]";
        }
        else if (xref is PdfStreamXRef)
        {
            return $"{xref.Id.Number}:{xref.Id.Generation} [grey](stream)[/]";
        }

        return $"{xref.Id.Number}:{xref.Id.Generation}";
    }
}