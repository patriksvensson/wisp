var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

////////////////////////////////////////////////////////////////
// Tasks

Task("Clean")
    .Does(context =>
{
    context.CleanDirectory("./.artifacts");
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(context => 
{
    DotNetBuild("./src/Wisp.sln", new DotNetBuildSettings {
        Configuration = configuration,
        Verbosity = DotNetVerbosity.Minimal,
        NoLogo = true,
        NoIncremental = context.HasArgument("rebuild"),
        EnvironmentVariables = new Dictionary<string, string> {
            { "MSBUILDTERMINALLOGGER", "auto" }
        },
        MSBuildSettings = new DotNetMSBuildSettings()
            .TreatAllWarningsAs(MSBuildTreatAllWarningsAs.Error)
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(context => 
{
    DotNetTest("./src/Wisp.Tests/Wisp.Tests.csproj", new DotNetTestSettings {
        Configuration = configuration,
        Verbosity = DotNetVerbosity.Minimal,
        NoLogo = true,
        NoRestore = true,
        NoBuild = true,
        EnvironmentVariables = new Dictionary<string, string> {
            { "MSBUILDTERMINALLOGGER", "auto" }
        },
    });
});

Task("Package")
    .IsDependentOn("Test")
    .Does(context => 
{
    context.DotNetPack($"./src/Wisp.sln", new DotNetPackSettings {
        Configuration = configuration,
        Verbosity = DotNetVerbosity.Minimal,
        NoLogo = true,
        NoRestore = true,
        NoBuild = true,
        OutputDirectory = "./.artifacts",
        EnvironmentVariables = new Dictionary<string, string> {
            { "MSBUILDTERMINALLOGGER", "auto" }
        },
        MSBuildSettings = new DotNetMSBuildSettings()
            .TreatAllWarningsAs(MSBuildTreatAllWarningsAs.Error)
    });
});

Task("Publish-NuGet")
    .WithCriteria(ctx => BuildSystem.IsRunningOnGitHubActions, "Not running on GitHub Actions")
    .IsDependentOn("Package")
    .Does(context => 
{
    var apiKey = Argument<string>("nuget-key", null);
    if(string.IsNullOrWhiteSpace(apiKey)) {
        throw new CakeException("No NuGet API key was provided.");
    }

    // Publish to GitHub Packages
    foreach(var file in context.GetFiles("./.artifacts/*.nupkg")) 
    {
        context.Information("Publishing {0}...", file.GetFilename().FullPath);
        DotNetNuGetPush(file.FullPath, new DotNetNuGetPushSettings
        {
            Source = "https://api.nuget.org/v3/index.json",
            ApiKey = apiKey,
        });
    }
});

////////////////////////////////////////////////////////////////
// Targets

Task("Publish")
    .IsDependentOn("Publish-NuGet");

Task("Default")
    .IsDependentOn("Package");

////////////////////////////////////////////////////////////////
// Execution

RunTarget(target)