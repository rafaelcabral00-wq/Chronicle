using System.Text.Json;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfPackageSourceTests
{
    [Fact]
    public void ManifestDeclaresProvisionalPackageIdentityAndCurrentSlice()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(PackageFile("Metadata", "werewolf.package-manifest.json")));
        var root = document.RootElement;

        Assert.Equal(WerewolfRuleSetPackage.ProvisionalPackageId, root.GetProperty("packageId").GetString());
        Assert.Equal("provisional-governance-pending", root.GetProperty("packageIdStatus").GetString());
        Assert.Equal("werewolf3e.character-creation.current-slice", root.GetProperty("declaredReleaseScope").GetProperty("scopeId").GetString());
        Assert.Equal("not-published", root.GetProperty("declaredReleaseScope").GetProperty("publicationStatus").GetString());
    }

    [Fact]
    public void CurrentSliceKeepsRequiredCapabilitiesDisabled()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(PackageFile("Metadata", "current-slice.json")));
        var disabled = document.RootElement.GetProperty("disabledCapabilities")
            .EnumerateArray()
            .Select(item => item.GetString())
            .ToArray();
        var enforcement = document.RootElement.GetProperty("enforcement");

        Assert.Contains("additional-gift-purchase", disabled);
        Assert.Contains("runtime-gift-execution", disabled);
        Assert.Equal("disabled", enforcement.GetProperty("additionalGiftPurchase").GetString());
        Assert.Equal("disabled", enforcement.GetProperty("runtimeGiftEffects").GetString());
    }

    [Fact]
    public void PackageSourceDoesNotCopyPrototypeReviewsOrEvidenceLedgers()
    {
        var packageRoot = PackageRoot();
        var forbiddenPathFragments = new[]
        {
            $"{Path.DirectorySeparatorChar}reviews{Path.DirectorySeparatorChar}",
            "review-record",
            "review-evidence",
            "review-ledger",
            "catalog-review",
            "prototype-readiness",
            "prototype-work-status"
        };

        var copiedEvidenceFiles = Directory.EnumerateFiles(packageRoot, "*", SearchOption.AllDirectories)
            .Where(path => forbiddenPathFragments.Any(fragment => path.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        Assert.Empty(copiedEvidenceFiles);
    }

    [Fact]
    public void WerewolfProjectReferencesOnlyAuthorizedAbstractionsAndContracts()
    {
        var projectReferences = LoadProjectReferences(PackageFile("Chronicle.RuleSets.Werewolf.csproj"));

        Assert.Equal(
            ["Chronicle.Contracts", "Chronicle.RuleSets.Abstractions"],
            projectReferences.Order(StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void WerewolfPackageHasNoForbiddenRuntimeDependencies()
    {
        var packageRoot = PackageRoot();
        var forbiddenTokens = new[]
        {
            "Chronicle.Persistence.Sqlite",
            "Chronicle.Presentation.Desktop",
            "Chronicle.NarrativeIntelligence.OpenAI",
            "Microsoft.EntityFrameworkCore",
            "OpenAI",
            "HttpClient",
            "SQLite",
            "DbContext"
        };

        var sourceText = Directory.EnumerateFiles(packageRoot, "*", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(File.ReadAllText);

        foreach (var token in forbiddenTokens)
        {
            Assert.DoesNotContain(sourceText, text => text.Contains(token, StringComparison.Ordinal));
        }
    }

    private static string[] LoadProjectReferences(string projectPath)
    {
        using var document = JsonDocument.Parse("{}");
        _ = document;

        var projectDirectory = Path.GetDirectoryName(projectPath) ?? throw new InvalidOperationException("Project path has no directory.");
        var lines = File.ReadAllLines(projectPath);

        return lines
            .Where(line => line.Contains("<ProjectReference Include=", StringComparison.Ordinal))
            .Select(line => line.Split('"')[1])
            .Select(include => Path.GetFullPath(include, projectDirectory))
            .Select(Path.GetFileNameWithoutExtension)
            .Where(projectName => projectName is not null)
            .Select(projectName => projectName!)
            .ToArray();
    }

    private static string PackageFile(params string[] pathParts)
    {
        return Path.Combine([PackageRoot(), .. pathParts]);
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
}
