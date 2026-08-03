using Chronicle.RuleSets.Abstractions.PackageSources;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class RuleSetPackageSourceValidatorTests
{
    [Fact]
    public void WerewolfPackageSourceIsValid()
    {
        var result = RuleSetPackageSourceValidator.Validate(PackageRoot());

        Assert.True(result.IsValid, FormatFindings(result));
        Assert.Empty(result.Findings);
        Assert.Equal(result.FileInventory.Order(StringComparer.Ordinal), result.FileInventory);
    }

    [Fact]
    public void MissingManifestIsRejected()
    {
        using var copy = PackageSourceCopy.Create();
        File.Delete(copy.PathTo("Metadata", "werewolf.package-manifest.json"));

        var result = RuleSetPackageSourceValidator.Validate(copy.Root);

        Assert.Contains(result.Findings, finding => finding.Code == RuleSetPackageSourceErrorCode.MissingManifest);
    }

    [Fact]
    public void MalformedManifestJsonIsRejected()
    {
        using var copy = PackageSourceCopy.Create();
        File.WriteAllText(copy.PathTo("Metadata", "werewolf.package-manifest.json"), "{ malformed");

        var result = RuleSetPackageSourceValidator.Validate(copy.Root);

        Assert.Contains(result.Findings, finding => finding.Code == RuleSetPackageSourceErrorCode.MalformedManifestJson);
    }

    [Fact]
    public void MissingLocalizationIsRejected()
    {
        using var copy = PackageSourceCopy.Create();
        File.Delete(copy.PathTo("Localization", "pt-BR", "current-slice.json"));

        var result = RuleSetPackageSourceValidator.Validate(copy.Root);

        Assert.Contains(result.Findings, finding => finding.Code == RuleSetPackageSourceErrorCode.MissingLocalizationResource);
    }

    [Fact]
    public void UndeclaredResourceIsRejected()
    {
        using var copy = PackageSourceCopy.Create();
        File.WriteAllText(copy.PathTo("Metadata", "extra.json"), "{}");

        var result = RuleSetPackageSourceValidator.Validate(copy.Root);

        Assert.Contains(result.Findings, finding => finding.Code == RuleSetPackageSourceErrorCode.UndeclaredResource);
    }

    [Fact]
    public void PathTraversalIsRejected()
    {
        using var copy = PackageSourceCopy.Create();
        var manifest = File.ReadAllText(copy.PathTo("Metadata", "werewolf.package-manifest.json"));
        manifest = manifest.Replace("\"pt-BR\"", "\"../outside\"", StringComparison.Ordinal);
        File.WriteAllText(copy.PathTo("Metadata", "werewolf.package-manifest.json"), manifest);

        var result = RuleSetPackageSourceValidator.Validate(copy.Root);

        Assert.Contains(result.Findings, finding => finding.Code == RuleSetPackageSourceErrorCode.PathTraversal);
    }

    [Fact]
    public void ProhibitedPrototypeEvidenceIsRejected()
    {
        using var copy = PackageSourceCopy.Create();
        Directory.CreateDirectory(copy.PathTo("reviews"));
        File.WriteAllText(copy.PathTo("reviews", "catalog-review-result.json"), "{}");

        var result = RuleSetPackageSourceValidator.Validate(copy.Root);

        Assert.Contains(result.Findings, finding => finding.Code == RuleSetPackageSourceErrorCode.ProhibitedPrototypeEvidence);
    }

    [Fact]
    public void ProtectedSourceMaterialIsRejected()
    {
        using var copy = PackageSourceCopy.Create();
        File.WriteAllText(copy.PathTo("Metadata", "source-note.json"), "{\"sourcebookTextIncluded\": true}");

        var result = RuleSetPackageSourceValidator.Validate(copy.Root);

        Assert.Contains(result.Findings, finding => finding.Code == RuleSetPackageSourceErrorCode.ProtectedSourceMaterial);
    }

    [Theory]
    [InlineData("secret.txt", "api_key=not-real")]
    [InlineData("generated.dll", "binary-placeholder")]
    [InlineData("generated.g.cs", "generated")]
    public void SecretGeneratedAndBinaryFilesAreRejected(string fileName, string content)
    {
        using var copy = PackageSourceCopy.Create();
        File.WriteAllText(copy.PathTo(fileName), content);

        var result = RuleSetPackageSourceValidator.Validate(copy.Root);

        Assert.Contains(result.Findings, finding =>
            finding.Code is RuleSetPackageSourceErrorCode.ForbiddenSecret or
                RuleSetPackageSourceErrorCode.ForbiddenBinary or
                RuleSetPackageSourceErrorCode.ForbiddenGeneratedOutput);
    }

    [Fact]
    public void DisabledOperationsMustRemainDisabled()
    {
        using var copy = PackageSourceCopy.Create();
        var manifest = File.ReadAllText(copy.PathTo("Metadata", "werewolf.package-manifest.json"));
        manifest = manifest.Replace("\"status\": \"disabled\"", "\"status\": \"enabled\"", StringComparison.Ordinal);
        File.WriteAllText(copy.PathTo("Metadata", "werewolf.package-manifest.json"), manifest);

        var result = RuleSetPackageSourceValidator.Validate(copy.Root);

        Assert.Contains(result.Findings, finding => finding.Code == RuleSetPackageSourceErrorCode.DisabledOperationNotEnforced);
    }

    [Fact]
    public void RepeatedValidationIsDeterministic()
    {
        var first = RuleSetPackageSourceValidator.Validate(PackageRoot());
        var second = RuleSetPackageSourceValidator.Validate(PackageRoot());

        Assert.Equal(FormatFindings(first), FormatFindings(second));
        Assert.Equal(first.IsValid, second.IsValid);
        Assert.Equal(first.FileInventory, second.FileInventory);
    }

    [Fact]
    public void ValidationDoesNotMutateFilesystem()
    {
        using var copy = PackageSourceCopy.Create();
        var before = Snapshot(copy.Root);

        _ = RuleSetPackageSourceValidator.Validate(copy.Root);

        Assert.Equal(before, Snapshot(copy.Root));
    }

    private static string[] Snapshot(string root)
    {
        return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/'))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static string FormatFindings(RuleSetPackageSourceValidationResult result)
    {
        return string.Join(
            Environment.NewLine,
            result.Findings.Select(finding => $"{finding.Code} {finding.Path}: {finding.Message}"));
    }

    private static string PackageRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "rule-sets", "Chronicle.RuleSets.Werewolf");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not find Werewolf package source root.");
    }

    private sealed class PackageSourceCopy : IDisposable
    {
        private PackageSourceCopy(string root)
        {
            Root = root;
        }

        public string Root { get; }

        public static PackageSourceCopy Create()
        {
            var root = Directory.CreateTempSubdirectory("chronicle-package-source-").FullName;
            CopyDirectory(PackageRoot(), root);
            return new PackageSourceCopy(root);
        }

        public string PathTo(params string[] pathParts)
        {
            return Path.Combine([Root, .. pathParts]);
        }

        public void Dispose()
        {
            Directory.Delete(Root, true);
        }

        private static void CopyDirectory(string source, string target)
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
                File.Copy(file, Path.Combine(target, Path.GetRelativePath(source, file)));
            }
        }
    }
}
