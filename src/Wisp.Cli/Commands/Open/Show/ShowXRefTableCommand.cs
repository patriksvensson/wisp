namespace Wisp.Cli.Show;

[UsedImplicitly]
public sealed class ShowXRefTableCommand : ShowCommand<ShowXRefTableCommand.Settings>
{
    [UsedImplicitly]
    public sealed class Settings : ShowSettings
    {
        public Settings(string input)
            : base(input)
        {
        }
    }

    protected override void Execute(CommandContext context, Settings settings, CosDocument document)
    {
        var table = AnsiConsole.Status()
            .Start("Building table...", _ =>
            {
                var table = new Table().RoundedBorder();
                table.AddColumn("[gray]ID[/]");
                table.AddColumn("[gray]XRef Kind[/]");
                table.AddColumn("[gray]Information[/]");

                var count = 0;
                foreach (var xref in document.XRefTable)
                {
                    if (xref is CosIndirectXRef indirect)
                    {
                        table.AddRow(
                            $"[blue]{xref.Id.Number}[/]:[blue]{xref.Id.Generation}[/]",
                            "[italic]Indirect[/]",
                            $"At position [blue]{indirect.Position}[/]");
                    }
                    else if (xref is CosStreamXRef stream)
                    {
                        table.AddRow(
                            $"[blue]{xref.Id.Number}[/]:[blue]{xref.Id.Generation}[/]",
                            "[italic]Stream[/]",
                            $"At index [green]{stream.Index}[/] in stream [blue]{stream.StreamId.Number}[/]:[blue]{stream.StreamId.Generation}[/]");
                    }

                    count++;
                }

                table.Caption($"[blue]{count}[/] entries");
                return table;
            });

        AnsiConsole.Write(table);
    }
}