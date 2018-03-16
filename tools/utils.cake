public struct CodeInfo
{
    public readonly string SemVer;        // 0.1.0-alpha
    public readonly string Version;       // 0.1.0
    public readonly string VersionSuffix; // alpha
    public readonly string Branch;        // master
    public readonly string Commit;        // 4468e5deabf5e6d0740cd1a77df56f67093ec943
    public readonly string ShortCommit;   // 4468e5de (first 8 chars)

    public CodeInfo(string semVer, string branch, string commit)
    {
        SemVer = semVer ?? string.Empty;
        Branch = branch ?? string.Empty;
        Commit = commit ?? string.Empty;
        ShortCommit = Commit.Length >= 8 ? Commit.Substring(0, 8) : string.Empty;

        string version;
        string suffix;
        ParseSemVer(SemVer, out version, out suffix);
        Version = version;
        VersionSuffix = suffix;
    }

    private static void ParseSemVer(string semVer, out string version, out string suffix)
    {
        var rx = new System.Text.RegularExpressions.Regex(@"^(?<version>\d+\.\d+\.\d+(.\d+)?)(\-(?<suffix>\w+))?$");
        var m = rx.Match(semVer ?? string.Empty);
        if (m.Success)
        {
            version = m.Groups["version"].Value;
            suffix = m.Groups["suffix"].Value;
            return;
        }

        version = "0.1.0";
        suffix = string.Empty;
    }
}

CodeInfo GetSemVerAndGitInfo()
{
    const string paramName = "Version";
    try
    {
        var v = GitVersion(new GitVersionSettings { RepositoryPath = "." });
        var semVer = HasArgument(paramName) ? Argument<string>(paramName) : v.SemVer;
        return new CodeInfo(semVer, v.BranchName, v.Sha);
    }
    catch (System.Exception ex)
    {
        var msg = string.Format("Failed to fetch git commit SHA and/or branch name; {0}", ex.Message);
        Warning(msg);
        var semVer = HasArgument(paramName) ? Argument<string>(paramName) : "0.1.0";
        return new CodeInfo(semVer, "unknown", "unknown");
    }
}

int GetBuildNumber()
{
    const string paramName = "BuildNumber";
    return
        HasArgument(paramName) ? Argument<int>(paramName) :
        AppVeyor.IsRunningOnAppVeyor ? AppVeyor.Environment.Build.Number :
        TravisCI.IsRunningOnTravisCI ? TravisCI.Environment.Build.BuildNumber :
        0;
}

DirectoryPath GetAbsDirPathFromArg(string argName, string defaultValue)
{
	return DirectoryPath.FromString(Argument(argName, defaultValue)).MakeAbsolute(Context.Environment);
}

FilePath GetSlnFile(string root)
{
    var normRoot = DirectoryPath.FromString(root).MakeAbsolute(Context.Environment).FullPath;
	var sln = GetFiles(normRoot + "/*.sln").FirstOrDefault();
    if (sln == null)
    {
        throw new Exception(string.Format("No solution file has been found in {0} (search is not recursive).", normRoot));
    }
    return sln;
}

FilePathCollection GetProjectFiles(string root)
{
    var normRoot = DirectoryPath.FromString(root).MakeAbsolute(Context.Environment).FullPath;
	return GetFiles(normRoot + "/**/*.csproj");
}

FilePathCollection GetTestProjectFiles(string root)
{
    var normRoot = DirectoryPath.FromString(root).MakeAbsolute(Context.Environment).FullPath;
	return GetFiles(normRoot + "/**/*.Tests.csproj");
}

FilePathCollection GetTestProjectDlls(string root, string configuration)
{
    var normRoot = DirectoryPath.FromString(root).MakeAbsolute(Context.Environment).FullPath;
	return GetFiles(string.Format("{0}/**/bin/{1}/**/*.Tests.dll", normRoot, configuration));
}

string GetLatestInstalledRuntimeVersion()
{
    var dotnetInfo = new System.Diagnostics.Process
    {
        StartInfo =
        {
            FileName = "dotnet",
            Arguments = "--info",
            UseShellExecute = false,
            CreateNoWindow = false,
            WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true
        }
    };
    var output = string.Empty;
    using (dotnetInfo)
    {
        dotnetInfo.Start();
        output = dotnetInfo.StandardOutput.ReadToEnd();
        dotnetInfo.WaitForExit();
    }

    const string host = ".NET Core Shared Framework Host";
    var idx = output.IndexOf(host);
    if (idx > 0)
    {
        var rx = new System.Text.RegularExpressions.Regex(@"Version\s*:\s*(\d\.\d\.\d)");
        var m = rx.Match(output, idx + host.Length);
        if (m.Success)
        {
            return m.Groups[1].Value;
        }
    }

    return null;
}

string GetFxVersionArgForXunit()
{
    var runtime = GetLatestInstalledRuntimeVersion();
    return runtime == null
        ? string.Empty
        : "-fxversion " + runtime;
}
