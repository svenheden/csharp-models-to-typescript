using System.IO;
using System.Linq;
using Directories;
using GithubActionsLogger;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.Npm;
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
    [Solution] Solution Solution;

    Target GetSemVer => _ => _
        .Executes(() =>
        {
            Log.Information("GitVersion = {Value}", GitVersion.AssemblySemVer);
        });

    Target EnsureCleanPublishDirectory => _ => _
        .Executes(() =>
        {
            DirectoryHelper.TryDeleteFolder(PublishFolder);
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
            DotNetTasks.DotNetBuild(_ =>
                _.SetNoRestore(true)
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
        .DependsOn(EnsureCleanPublishDirectory)
        .DependsOn(Test)
        .DependsOn(Compile)
        .DependsOn(GetSemVer)
        .Triggers(NpmPack)
        .Executes(() =>
        {
            var project = Solution.GetProject("csharp-models-to-json");
            DotNetTasks.DotNetPublish(_ =>
                _.SetProject(project)
                    .SetOutput(PublishFolder)
                    .SetAssemblyVersion(GitVersion.AssemblySemVer)
            );
        });


    static readonly AbsolutePath DistFolder = RootDirectory / "dist";
    static readonly AbsolutePath ReleaseFolder = RootDirectory / "release";

    Target NpmRelease => _ => _
        .DependsOn(NpmPack)
        .Produces(ReleaseFolder)
        .Executes(() =>
        {
            Directory.CreateDirectory(ReleaseFolder);
            var releasePackage = Directory.GetFiles(DistFolder).Where(d => d.EndsWith(".tgz"))
                .ToList();
            releasePackage.ForEach(f => CopyFile(f, ReleaseFolder));
            // push to npm
        });

    Target NpmPack => _ => _
        .DependsOn(Publish)
        .Executes(() =>
        {
            DirectoryHelper.TryDeleteFolder(DistFolder);
            Directory.CreateDirectory(DistFolder);
            CopyFileToDist("package.json");
            var jsFiles = Directory.GetFiles(RootDirectory)
                .Where(d => d.EndsWith(".js"))
                .ToList();
            jsFiles.ForEach(CopyFileToDist);
            DirectoryHelper.CopyDirectory(PublishFolder,
                DistFolder +
                Path.DirectorySeparatorChar +
                Path.GetRelativePath(RootDirectory, DistFolder),
                true);

            NpmTasks.Npm($"version {GitVersion.FullSemVer} --allow-same-version", DistFolder);
            NpmTasks.Npm($"pack {DistFolder} --loglevel=warn", RootDirectory);
        });

    void CopyFileToDist(string fileName) =>
        CopyFile(fileName, DistFolder);

    void CopyFile(string fileName, string targetFolder) => File.Copy(fileName,
        targetFolder + Path.DirectorySeparatorChar + Path.GetFileName(fileName));
}