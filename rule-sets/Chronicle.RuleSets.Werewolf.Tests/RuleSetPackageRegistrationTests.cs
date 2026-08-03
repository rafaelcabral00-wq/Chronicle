using Chronicle.RuleSets.Abstractions.PackageSources;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class RuleSetPackageRegistrationTests
{
    [Fact]
    public void RegistersDiscoveredWerewolfPackage()
    {
        var package = DiscoverWerewolf();

        var result = Register(package);

        var registered = Assert.Single(result.AvailablePackages);
        Assert.Empty(result.RejectedPackages);
        Assert.Equal(RuleSetPackageRegistrationStatus.Available, result.Status);
        Assert.Equal("chronicle.rulesets.werewolf", registered.PackageId);
        Assert.Equal("0.1.0-source-skeleton", registered.PackageVersion);
        Assert.Equal("werewolf3e.character-creation.current-slice", registered.DeclaredScopeId);
        Assert.Equal("0.1.0", registered.MinimumChronicleVersion);
        Assert.Equal("same-major-contract", registered.MaximumChronicleVersionPolicy);
        Assert.Equal(1, registered.RuleSetContractVersion);
        Assert.Equal(["en", "pt-BR"], registered.SupportedLocales);
        Assert.Contains("character-creation", registered.Capabilities);
        Assert.Contains("character-creation.purchase-additional-gift", registered.DisabledOperations);
    }

    [Fact]
    public void RejectsNonvalidatedDescriptor()
    {
        var fabricated = DiscoverWerewolf() with { ValidationEvidence = string.Empty };

        var result = Register(fabricated);

        var rejection = Assert.Single(result.RejectedPackages);
        Assert.Empty(result.AvailablePackages);
        Assert.Equal(RuleSetPackageRegistrationErrorCode.MissingValidationEvidence, rejection.Code);
    }

    [Fact]
    public void RejectsDuplicatePackageIdAndVersion()
    {
        var package = DiscoverWerewolf();
        var duplicate = package with { PackageSourcePath = $"{package.PackageSourcePath}-copy" };

        var result = Register(package, duplicate);

        Assert.Empty(result.AvailablePackages);
        Assert.Equal(2, result.RejectedPackages.Count);
        Assert.All(result.RejectedPackages, rejection => Assert.Equal(RuleSetPackageRegistrationErrorCode.DuplicatePackageIdentity, rejection.Code));
    }

    [Fact]
    public void RegistersMultipleVersionsOfOnePackageId()
    {
        var package = DiscoverWerewolf();
        var later = package with { PackageVersion = "0.2.0-source-skeleton", PackageSourcePath = $"{package.PackageSourcePath}-v2" };

        var result = Register(package, later);

        Assert.Empty(result.RejectedPackages);
        Assert.Equal(2, result.AvailablePackages.Count);
        Assert.Equal(["0.1.0-source-skeleton", "0.2.0-source-skeleton"], result.Catalog.FindByPackageId(package.PackageId).Select(item => item.PackageVersion).ToArray());
    }

    [Fact]
    public void DistinguishesCompatibleAndIncompatibleContractVersions()
    {
        var package = DiscoverWerewolf();
        var incompatible = package with
        {
            PackageId = "chronicle.rulesets.future",
            PackageVersion = "2.0.0-source-skeleton",
            RuleSetContractVersion = 2,
            PackageSourcePath = $"{package.PackageSourcePath}-future"
        };

        var result = Register(package, incompatible);

        Assert.Single(result.CompatiblePackages);
        Assert.Single(result.IncompatiblePackages);
        Assert.Equal("chronicle.rulesets.future", result.IncompatiblePackages[0].PackageId);
    }

    [Fact]
    public void CatalogResultsAreImmutableSnapshots()
    {
        var package = DiscoverWerewolf();
        var result = Register(package);
        var first = result.Catalog.Packages;

        _ = first.ToList();

        var second = result.Catalog.Packages;
        Assert.NotSame(first, second);
        Assert.Equal(first, second);
    }

    [Fact]
    public void RegistrationOrderingIsDeterministic()
    {
        var package = DiscoverWerewolf();
        var beta = package with { PackageId = "chronicle.rulesets.beta", PackageSourcePath = $"{package.PackageSourcePath}-beta" };
        var alpha = package with { PackageId = "chronicle.rulesets.alpha", PackageSourcePath = $"{package.PackageSourcePath}-alpha" };

        var first = Register(beta, alpha);
        var second = Register(alpha, beta);

        Assert.Equal(Format(first), Format(second));
        Assert.Equal(["chronicle.rulesets.alpha", "chronicle.rulesets.beta"], first.AvailablePackages.Select(item => item.PackageId).ToArray());
    }

    [Fact]
    public void CatalogLooksUpByIdentityAndVersion()
    {
        var package = DiscoverWerewolf();
        var result = Register(package);

        Assert.Single(result.Catalog.FindByPackageId("chronicle.rulesets.werewolf"));
        var exact = result.Catalog.FindByPackageIdAndVersion("chronicle.rulesets.werewolf", "0.1.0-source-skeleton");

        Assert.NotNull(exact);
        Assert.Equal(package.PackageSourcePath, exact.PackageSourcePath);
    }

    [Fact]
    public void RegistrationDoesNotMutateFilesystemOrPerformImplicitDiscovery()
    {
        using var root = TemporaryDirectory.Create();
        var package = DiscoverWerewolf() with { PackageSourcePath = root.Path };
        var before = Snapshot(root.Path);

        _ = Register(package);

        Assert.Equal(before, Snapshot(root.Path));
    }

    [Fact]
    public void RegistrationDoesNotRequireForbiddenRuntimeDependencies()
    {
        var projectFile = Path.Combine(FindRepositoryRoot(), "src", "Chronicle.RuleSets.Abstractions", "Chronicle.RuleSets.Abstractions.csproj");
        var projectText = File.ReadAllText(projectFile);
        var forbidden = new[]
        {
            "Chronicle.Persistence",
            "Chronicle.Presentation",
            "Chronicle.NarrativeIntelligence.OpenAI",
            "Microsoft.EntityFrameworkCore",
            "OpenAI",
            "HttpClient",
            "DbContext"
        };

        Assert.DoesNotContain(forbidden, token => projectText.Contains(token, StringComparison.Ordinal));
    }

    [Fact]
    public void FocusedDiscoveryRegistrationLookupFlowSucceeds()
    {
        var discovery = RuleSetPackageSourceDiscoveryService.Discover(new RuleSetPackageSourceDiscoveryRequest([RuleSetsRoot()]));
        var registration = RuleSetPackageRegistrationService.Register(new RuleSetPackageRegistrationRequest(discovery.ValidatedPackages, 1));

        var package = registration.Catalog.FindByPackageIdAndVersion("chronicle.rulesets.werewolf", "0.1.0-source-skeleton");

        Assert.NotNull(package);
        Assert.Equal("werewolf3e.character-creation.current-slice", package.DeclaredScopeId);
        Assert.Empty(registration.RejectedPackages);
    }

    private static RuleSetPackageRegistrationResult Register(params RuleSetPackageSourceDescriptor[] packages)
    {
        return RuleSetPackageRegistrationService.Register(new RuleSetPackageRegistrationRequest(packages, 1));
    }

    private static RuleSetPackageSourceDescriptor DiscoverWerewolf()
    {
        var result = RuleSetPackageSourceDiscoveryService.Discover(new RuleSetPackageSourceDiscoveryRequest([RuleSetsRoot()]));
        return Assert.Single(result.ValidatedPackages);
    }

    private static string Format(RuleSetPackageRegistrationResult result)
    {
        var packages = result.AvailablePackages.Select(package => $"{package.PackageId}|{package.PackageVersion}|{package.PackageSourcePath}");
        var rejections = result.RejectedPackages.Select(rejection => $"{rejection.Code}|{rejection.PackageId}|{rejection.PackageVersion}|{rejection.PackageSourcePath}");
        return string.Join(Environment.NewLine, packages.Concat(rejections));
    }

    private static string[] Snapshot(string root)
    {
        return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/'))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static string RuleSetsRoot()
    {
        return Path.Combine(FindRepositoryRoot(), "rule-sets");
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

    private sealed class TemporaryDirectory : IDisposable
    {
        private TemporaryDirectory(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public static TemporaryDirectory Create()
        {
            return new TemporaryDirectory(Directory.CreateTempSubdirectory("chronicle-registration-").FullName);
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
