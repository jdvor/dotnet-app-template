#load "tools/utils.cake"
#tool "nuget:?package=GitVersion.CommandLine"

const string Clean = "Clean";
const string Restore = "Restore";
const string Build = "Build";
const string ReBuild = "ReBuild";
const string Test = "Test";
const string Pack = "Pack";

const string Src = "./src";
const string TestSrc = "./test";


//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("Target", Pack);
var configuration = Argument("Configuration", "Release");
var verbosity = Argument<DotNetCoreVerbosity>("Verbosity", DotNetCoreVerbosity.Minimal);
var artifactsPath = GetAbsDirPathFromArg("Artifacts", "./artifacts");
var testResultsPath = GetAbsDirPathFromArg("TestResults", "./test-results");


//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var solutionPath = GetSlnFile(".").FullPath;
var buildNumber = GetBuildNumber();
var codeInfo = GetSemVerAndGitInfo();
var infoStr = string.Format("{0} semver: {1}, build: {2}, git: {3} / {4})",
    configuration.ToUpperInvariant(), codeInfo.SemVer, buildNumber, codeInfo.ShortCommit, codeInfo.Branch);
Information(infoStr);


//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task(Clean)
    .Description("Deletes contents of build directories (bin, obj, artifacts and test results).")
    .Does(() =>
{
    var settings = new DeleteDirectorySettings { Recursive = true, Force = true };
    if (DirectoryExists(artifactsPath)) DeleteDirectory(artifactsPath, settings);
    if (DirectoryExists(testResultsPath)) DeleteDirectory(testResultsPath, settings);
    DeleteDirectories(GetDirectories("./**/bin"), settings);
    DeleteDirectories(GetDirectories("./**/obj"), settings);
});


Task(Restore)
    .Description("Restores NuGet packages.")
    .Does(() =>
{
    DotNetCoreRestore(solutionPath);
});


Task(Build)
    .Description("Build all projects linked by solution file.")
    .Does(() =>
{
    var buildSettings = new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        NoRestore = true,
        Verbosity = verbosity,
        MSBuildSettings = new DotNetCoreMSBuildSettings
        {
            ArgumentCustomization = args => args
                                    .Append("-p:SemVer=" + codeInfo.SemVer)
                                    .Append("-p:Version=" + codeInfo.Version)
                                    .Append("-p:BuildNumber=" + buildNumber)
                                    .Append("-p:GitCommit=" + codeInfo.ShortCommit),
            NoLogo = true,
            Verbosity = verbosity,
            DetailedSummary = false,
        }
    };
    DotNetCoreBuild(solutionPath, buildSettings);
});


Task(ReBuild)
    .Description("Combines tasks Clean, Restore and Build (in that order). It's convenience target to be used in Visual Studio Code tasks.json.")
    .IsDependentOn(Clean)
    .IsDependentOn(Restore)
    .IsDependentOn(Build)
    .Does(() =>
{
});


Task(Test)
    .Description("Executes all tests; runs first: Build.")
    .IsDependentOn(Build)
    .Does(() =>
{
    EnsureDirectoryExists(testResultsPath);
    var fxversion = GetFxVersionArgForXunit();
    var projects = GetTestProjectFiles(TestSrc);
    foreach(var project in projects)
    {
        var xml = testResultsPath.CombineWithFilePath(project.GetFilenameWithoutExtension()).FullPath + ".xml";
        var args = string.Format("-configuration {0} -nobuild -nologo {1} -xml {2}", configuration, fxversion, xml);
        DotNetCoreTool(
            projectPath: project.FullPath,
            command: "xunit",
            arguments: args
        );
    }
});


Task(Pack)
    .Description("Packages NuGet packages; runs first: Clean, Restore, Build, Test.")
    .IsDependentOn(Clean)
    .IsDependentOn(Restore)
    .IsDependentOn(Build)
    .IsDependentOn(Test)
    .Does(() =>
{
    EnsureDirectoryExists(artifactsPath);

    // var revision = buildNumber.ToString("D4");
    // var projects = GetProjectFiles(Src);
    // foreach (var project in projects)
    // {
    //     DotNetCorePack(
    //         project.FullPath,
    //         new DotNetCorePackSettings()
    //         {
    //             Configuration = configuration,
    //             OutputDirectory = artifactsPath,
    //             NoRestore = true,
    //             NoBuild = true,
    //             Verbosity = verbosity,
    //         });
    // }
});


RunTarget(target);
