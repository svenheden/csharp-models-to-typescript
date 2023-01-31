using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

namespace GithubActionsLogger;

[PublicAPI]
public static class DotNetTestSettingsExtensions
{
    public static DotNetTestSettings AddGithubActionsLogger(this DotNetTestSettings toolSettings,
        string resultsDirectory)
    {
        Assert.True(GitHubActions.Instance != null);
        var githubActionsPackage = NuGetPackageResolver
            .GetLocalInstalledPackage("GitHubActionsTestLogger", ToolPathResolver.NuGetPackagesConfigFile)
            .NotNull("githubActionsPackage != null");
        var loggerPath = githubActionsPackage.Directory / "lib";
        Assert.DirectoryExists(loggerPath);
        return toolSettings
            .SetResultsDirectory(resultsDirectory)
            .SetLoggers("GitHubActions")
            .SetTestAdapterPath(loggerPath);
    }
}