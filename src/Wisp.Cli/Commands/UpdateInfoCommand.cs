using System.Reflection.Metadata;

namespace Wisp.Cli;

[UsedImplicitly]
public sealed class UpdateInfoCommand : Command<UpdateInfoCommand.Setting>
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

        public Setting(string input, string output, string? title, string? author)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Output = output ?? throw new ArgumentNullException(nameof(output));
            Title = title;
            Author = author;
        }
    }

    public override int Execute(CommandContext context, Setting settings)
    {
        // Open and edit the document
        var document = CosDocument.Open(File.OpenRead(settings.Input));
        document.Info.Title = new CosString(settings.Title ?? string.Empty);
        document.Info.Author = new CosString(settings.Author ?? string.Empty);

        // Save the document
        document.Save(File.OpenWrite(settings.Output));

        return 0;
    }
}