var target = Argument("target", "Test");
var configuration = Argument("configuration", "Release");

var version = Argument("app-version", "");

Task("Clean")
    .Does(() =>
{
    CleanDirectory($"./Source/TranslationViewer/bin/{configuration}");
    CleanDirectory("./Setup/Files");
    CleanDirectory("./.artifacts");
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetBuild("./Source/TranslationViewer.sln", new DotNetBuildSettings
    {
        Configuration = configuration,
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetTest("./Source/TranslationViewer.sln", new DotNetTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
    });
});

Task("Publish")
    .IsDependentOn("Test")
    .Does(() =>
{
    if (string.IsNullOrWhiteSpace(version))
    {
        throw new CakeException("No app version specified.");
    }

    string actualVersion = version;

    if (version.StartsWith("v"))
    {
        actualVersion = version.Substring(1);
    }

    DotNetPublish("./Source/TranslationViewer/TranslationViewer.csproj", new DotNetPublishSettings
    {
        NoRestore = true,
        OutputDirectory = "./Setup/Files",
        Configuration = configuration,
        MSBuildSettings = new DotNetMSBuildSettings()
            .WithProperty("Version", actualVersion)
    });
});

Task("Setup")
    .IsDependentOn("Publish")
    .Does(() =>
{
    string actualVersion = version;

    if (version.StartsWith("v"))
    {
        actualVersion = version.Substring(1);
    }

    var settings = new InnoSetupSettings
    {
        OutputDirectory = "./.artifacts",
        OutputBaseFilename = $"TranslationViewerSetup-v{actualVersion}",
        ArgumentCustomization = arg => arg.Append("/DMyAppVersion=" + actualVersion),
    };
        
    InnoSetup("./Setup/script.iss", settings);
});

RunTarget(target);