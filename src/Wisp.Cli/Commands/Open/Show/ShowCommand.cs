namespace Wisp.Cli;

public abstract class ShowCommand<T> : OpenCommand<T>
    where T : ShowSettings
{
}

public abstract class ShowSettings : OpenSettings
{
    protected ShowSettings(string input)
        : base(input)
    {
    }
}