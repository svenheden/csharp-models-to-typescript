using GithubActionsLogger;
using Microsoft.Build.Construction;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Serilog;

[GitHubActions(
    nameof(Build),
    GitHubActionsImage.UbuntuLatest,
    FetchDepth = 0,
    On = new[] { GitHubActionsTrigger.Push },
    InvokedTargets = new[] { nameof(Publish) },
    PublishArtifacts = true,
    EnableGitHubToken = true
)]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitVersion] readonly GitVersion GitVersion;

    Target GetSemVer => _ => _
        .Executes(() =>
        {
            Log.Information("GitVersion = {Value}", GitVersion.AssemblySemVer);
        });

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore();
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetTasks.DotNetBuild(b =>
                b.SetNoRestore(true)
            );
        });

    static readonly string ResultsDirectory = RootDirectory / "results";

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTasks.DotNetTest(_ =>
                _.EnableNoBuild()
                    .SetNoRestore(true)
                    .When(IsServerBuild, _ => _
                        .AddGithubActionsLogger(ResultsDirectory)
                    )
            );
        });

    static readonly string PublishFolder = RootDirectory / "publish";

    Target Publish => _ => _
        .DependsOn(Compile)
        .DependsOn(Test)
        .DependsOn(GetSemVer)
        .Executes(() =>
        {
            DotNetTasks.DotNetPublish(s =>
                s.SetOutput(PublishFolder)
                    .SetAssemblyVersion(GitVersion.AssemblySemVer)
            );
        });
}