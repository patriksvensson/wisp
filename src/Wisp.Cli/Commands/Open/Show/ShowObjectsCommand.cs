namespace Wisp.Cli.Show;

[UsedImplicitly]
public sealed class ShowObjectsCommand : ShowCommand<ShowObjectsCommand.Settings>
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
                table.AddColumn("Id");
                table.AddColumn("Kind");
                table.AddColumn("Information");

                var count = 0;
                var streamItemCount = 0;
                foreach (var obj in document.Objects)
                {
                    table.AddRow(
                        new Markup($"[blue]{obj.Id.Number}[/]:[blue]{obj.Id.Generation}[/]"),
                        new Markup("[italic]" + GetTypeName(obj) + "[/]"),
                        Text.Empty);

                    if (obj.Object is CosObjectStream objStream)
                    {
                        var subTable = new Table().NoBorder();
                        subTable.AddColumn("ID");
                        subTable.AddColumn("Kind");

                        for (var i = 0; i < objStream.N; i++)
                        {
                            var streamItem = objStream.GetObjectByIndex(document.Objects, i);
                            table.AddRow(
                                $"[blue]{streamItem.Id.Number}[/]:[blue]{streamItem.Id.Generation}[/]",
                                "[italic]" + GetTypeName(streamItem) + "[/]",
                                $"[gray]Child of[/] [blue]{obj.Id.Number}[/]:[blue]{obj.Id.Generation}[/]");

                            streamItemCount++;
                        }
                    }

                    count++;
                }

                table.Caption($"[green]{count}[/] items, [green]{streamItemCount}[/] stream items");
                return table;
            });

        AnsiConsole.Write(table);
    }

    private string GetTypeName(CosObject obj)
    {
        return CosTypeNameResolver.GetName(obj.Object);
    }
}