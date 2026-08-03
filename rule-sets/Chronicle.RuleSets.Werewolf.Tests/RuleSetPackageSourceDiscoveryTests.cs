using Chronicle.RuleSets.Abstractions.PackageSources;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class RuleSetPackageSourceDiscoveryTests
{
    [Fact]
    public void DiscoversValidWerewolfPackageFromRuleSetsRoot()
    {
        var result = RuleSetPackageSourceDiscoveryService.Discover(new RuleSetPackageSourceDiscoveryRequest([RuleSetsRoot()]));

        var package = Assert.Single(result.ValidatedPackages);
        Assert.Empty(result.RejectedPackages);
        Assert.Equal("chronicle.rulesets.werewolf", package.PackageId);
        Assert.Equal("provisional-governance-pending", package.PackageIdStatus);
        Assert.Equal("0.1.0-source-skeleton", package.PackageVersion);
        Assert.Equal("werewolf3e.character-creation.current-slice", package.DeclaredScopeId);
        Assert.Equal(RuleSetPackageSourceDiscoveryValidationStatus.Valid, package.ValidationStatus);
        Assert.Contains("character-creation", package.Capabilities);
    }

    [Fact]
    public void EmptyRootReturnsNoCandidates()
    {
        using var root = TemporaryDirectory.Create();

        var result = RuleSetPackageSourceDiscoveryService.Discover(new RuleSetPackageSourceDiscoveryRequest([root.Path]));

        Assert.Empty(result.ValidatedPackages);
        Assert.Empty(result.RejectedPackages);
    }

    [Fact]
    public void NonexistentRootIsRejected()
    {
        var missing = Path.Combine(Path.GetTempPath(), $"chronicle-missing-root-{Guid.NewGuid():N}");

        var result = RuleSetPackageSourceDiscoveryService.Discover(new RuleSetPackageSourceDiscoveryRequest([missing]));

        var rejection = Assert.Single(result.RejectedPackages);
        Assert.Empty(result.ValidatedPackages);
        Assert.Equal(RuleSetPackageSourceDiscoveryErrorCode.SearchRootMissing, rejection.Code);
    }

    [Fact]
    public void MalformedCandidateIsRejected()
    {
        using var root = TemporaryDirectory.Create();
        var candidate = Path.Combine(root.Path, "Malformed");
        Directory.CreateDirectory(Path.Combine(candidate, "Metadata"));
        File.WriteAllText(Path.Combine(candidate, "Metadata", "werewolf.package-manifest.json"), "{ malformed");

        var result = RuleSetPackageSourceDiscoveryService.Discover(new RuleSetPackageSourceDiscoveryRequest([root.Path]));

        var rejection = Assert.Single(result.RejectedPackages);
        Assert.Empty(result.ValidatedPackages);
        Assert.Equal(RuleSetPackageSourceDiscoveryErrorCode.MalformedCandidate, rejection.Code);
    }

    [Fact]
    public void ValidAndInvalidCandidatesAreReportedTogether()
    {
        using var root = TemporaryDirectory.Create();
        CopyPackage(PackageRoot(), Path.Combine(root.Path, "Valid"));
        CopyPackage(PackageRoot(), Path.Combine(root.Path, "Invalid"));
        File.WriteAllText(Path.Combine(root.Path, "Invalid", "unexpected.txt"), "undeclared");

        var result = RuleSetPackageSourceDiscoveryService.Discover(new RuleSetPackageSourceDiscoveryRequest([root.Path]));

        Assert.Single(result.ValidatedPackages);
        var rejection = Assert.Single(result.RejectedPackages);
        Assert.Equal(RuleSetPackageSourceDiscoveryErrorCode.ValidationFailed, rejection.Code);
        Assert.Contains(rejection.Findings, finding => finding.Code == RuleSetPackageSourceErrorCode.UndeclaredResource);
    }

    [Fact]
    public void DuplicateIdentityVersionCandidatesAreRejected()
    {
        using var root = TemporaryDirectory.Create();
        CopyPackage(PackageRoot(), Path.Combine(root.Path, "A"));
        CopyPackage(PackageRoot(), Path.Combine(root.Path, "B"));

        var result = RuleSetPackageSourceDiscoveryService.Discover(new RuleSetPackageSourceDiscoveryRequest([root.Path]));

        Assert.Empty(result.ValidatedPackages);
        Assert.Equal(2, result.RejectedPackages.Count);
        Assert.All(result.RejectedPackages, rejection => Assert.Equal(RuleSetPackageSourceDiscoveryErrorCode.DuplicatePackageIdentity, rejection.Code));
    }

    [Fact]
    public void DiscoveryOrderingIsDeterministic()
    {
        using var root = TemporaryDirectory.Create();
        CopyPackage(PackageRoot(), Path.Combine(root.Path, "B"));
        CopyPackage(PackageRoot(), Path.Combine(root.Path, "A"));
        RewritePackageIdentity(Path.Combine(root.Path, "A"), "chronicle.rulesets.alpha", "0.1.0-source-skeleton");
        RewritePackageIdentity(Path.Combine(root.Path, "B"), "chronicle.rulesets.beta", "0.1.0-source-skeleton");

        var first = RuleSetPackageSourceDiscoveryService.Discover(new RuleSetPackageSourceDiscoveryRequest([root.Path]));
        var second = RuleSetPackageSourceDiscoveryService.Discover(new RuleSetPackageSourceDiscoveryRequest([root.Path]));

        Assert.Equal(Format(first), Format(second));
        Assert.Equal(
            ["chronicle.rulesets.alpha", "chronicle.rulesets.beta"],
            first.ValidatedPackages.Select(package => package.PackageId).ToArray());
    }

    [Fact]
    public void DiscoveryDoesNotTraverseOutsideAuthorizedRoots()
    {
        using var authorizedRoot = TemporaryDirectory.Create();
        using var outsideRoot = TemporaryDirectory.Create();
        CopyPackage(PackageRoot(), Path.Combine(outsideRoot.Path, "Outside"));

        var result = RuleSetPackageSourceDiscoveryService.Discover(new RuleSetPackageSourceDiscoveryRequest([authorizedRoot.Path]));

        Assert.Empty(result.ValidatedPackages);
        Assert.Empty(result.RejectedPackages);
    }

    [Fact]
    public void DiscoveryDoesNotMutateFilesystem()
    {
        using var root = TemporaryDirectory.Create();
        CopyPackage(PackageRoot(), Path.Combine(root.Path, "Werewolf"));
        var before = Snapshot(root.Path);

        _ = RuleSetPackageSourceDiscoveryService.Discover(new RuleSetPackageSourceDiscoveryRequest([root.Path]));

        Assert.Equal(before, Snapshot(root.Path));
    }

    [Fact]
    public void DiscoveryDoesNotRequireForbiddenRuntimeDependencies()
    {
        var projectFile = Path.Combine(FindRepositoryRoot(), "src", "Chronicle.RuleSets.Abstractions", "Chronicle.RuleSets.Abstractions.csproj");
        var projectText = File.ReadAllText(projectFile);
        var forbidden = new[]
        {
            "Chronicle.Persistence",
            "Chronicle.Presentation",
            "Chronicle.NarrativeIntelligence.OpenAI",
            "Microsoft.EntityFrameworkCore",
            "OpenAI"
        };

        Assert.DoesNotContain(forbidden, token => projectText.Contains(token, StringComparison.Ordinal));
    }

    private static string Format(RuleSetPackageSourceDiscoveryResult result)
    {
        var packages = result.ValidatedPackages.Select(package => $"{package.PackageId}|{package.PackageVersion}|{package.PackageSourcePath}");
        var rejections = result.RejectedPackages.Select(rejection => $"{rejection.Code}|{rejection.PackageSourcePath}|{rejection.Message}");
        return string.Join(Environment.NewLine, packages.Concat(rejections));
    }

    private static void RewritePackageIdentity(string packageRoot, string packageId, string packageVersion)
    {
        var manifestPath = Path.Combine(packageRoot, "Metadata", "werewolf.package-manifest.json");
        var text = File.ReadAllText(manifestPath);
        text = text.Replace("\"packageId\": \"chronicle.rulesets.werewolf\"", $"\"packageId\": \"{packageId}\"", StringComparison.Ordinal);
        text = text.Replace("\"packageVersion\": \"0.1.0-source-skeleton\"", $"\"packageVersion\": \"{packageVersion}\"", StringComparison.Ordinal);
        File.WriteAllText(manifestPath, text);
    }

    private static string[] Snapshot(string root)
    {
        return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/'))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static string RuleSetsRoot()
    {
        return Path.Combine(FindRepositoryRoot(), "rule-sets");
    }

    private static string PackageRoot()
    {
        return Path.Combine(RuleSetsRoot(), "Chronicle.RuleSets.Werewolf");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Chronicle.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not find repository root from test base directory.");
    }

    private static void CopyPackage(string source, string target)
    {
        Directory.CreateDirectory(target);

        foreach (var directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories)
                     .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                     .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)))
        {
            Directory.CreateDirectory(Path.Combine(target, Path.GetRelativePath(source, directory)));
        }

        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories)
                     .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                     .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)))
        {
            var destination = Path.Combine(target, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(destination) ?? throw new InvalidOperationException("Destination file has no directory."));
            File.Copy(file, destination);
        }
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        private TemporaryDirectory(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public static TemporaryDirectory Create()
        {
            return new TemporaryDirectory(Directory.CreateTempSubdirectory("chronicle-discovery-").FullName);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
