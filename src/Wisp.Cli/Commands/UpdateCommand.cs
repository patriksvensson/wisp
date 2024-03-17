using System.Reflection.Metadata;

namespace Wisp.Cli;

[UsedImplicitly]
public sealed class Update : Command<Update.Setting>
{
    [UsedImplicitly]
    public sealed class Setting : CommandSettings
    {
        [CommandArgument(0, "<INPUT>")]
        public string Input { get; set; }

        [CommandArgument(1, "<OUTPUT>")]
        public string Output { get; set; }

        [CommandOption("--title <TITLE>")]
        public string? Title { get; set; }

        [CommandOption("--author <AUTHOR>")]
        public string? Author { get; set; }

        [CommandOption("--unpack")]
        public bool Unpack { get; set; }

        public Setting(string input, string output, string? title, string? author, bool unpack)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Output = output ?? throw new ArgumentNullException(nameof(output));
            Title = title;
            Author = author;
            Unpack = unpack;
        }
    }

    public override int Execute(CommandContext context, Setting settings)
    {
        var document = CosDocument.Open(File.OpenRead(settings.Input));

        if (settings.Title != null)
        {
            document.Info.Title = new CosString(settings.Title);
        }

        if (settings.Author != null)
        {
            document.Info.Author = new CosString(settings.Author);
        }

        document.Save(
            File.OpenWrite(settings.Output),
            unpack: settings.Unpack);

        return 0;
    }
}