using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class RuleSetRuntimeRegistryTests
{
    [Fact]
    public void RegistersWerewolfRuntimeAfterDiscoveryAndPackageRegistration()
    {
        var catalog = RegisteredCatalog();

        var result = RuleSetRuntimeRegistrationService.Register(new RuleSetRuntimeRegistrationRequest(catalog, [new WerewolfReferenceRuntime()]));

        var runtime = Assert.Single(result.RegisteredRuntimes);
        Assert.Empty(result.RejectedRuntimes);
        Assert.Equal(WerewolfRuleSetPackage.ProvisionalPackageId, runtime.Identity.PackageId);
        Assert.Equal(WerewolfRuleSetPackage.PackageVersion, runtime.Identity.PackageVersion);
    }

    [Fact]
    public void RejectsRuntimeIdentityVersionMismatch()
    {
        var catalog = RegisteredCatalog();
        var runtime = new TestRuntime(new WerewolfReferenceRuntime().Metadata with
        {
            Identity = new RuleSetRuntimeIdentity(WerewolfRuleSetPackage.ProvisionalPackageId, "9.9.9", "Mismatch", 1)
        });

        var result = RuleSetRuntimeRegistrationService.Register(new RuleSetRuntimeRegistrationRequest(catalog, [runtime]));

        var rejection = Assert.Single(result.RejectedRuntimes);
        Assert.Equal(RuleSetRuntimeRegistrationErrorCode.MissingRegisteredPackage, rejection.Code);
    }

    [Fact]
    public void RejectsIncompatibleRuntimeContractVersion()
    {
        var catalog = RegisteredCatalog();
        var baseRuntime = new WerewolfReferenceRuntime();
        var runtime = new TestRuntime(baseRuntime.Metadata with
        {
            Identity = baseRuntime.Metadata.Identity with { SupportedRuleSetContractVersion = 2 }
        });

        var result = RuleSetRuntimeRegistrationService.Register(new RuleSetRuntimeRegistrationRequest(catalog, [runtime]));

        var rejection = Assert.Single(result.RejectedRuntimes);
        Assert.Equal(RuleSetRuntimeRegistrationErrorCode.IncompatibleRuntime, rejection.Code);
    }

    [Fact]
    public void RejectsDuplicateRuntimeRegistration()
    {
        var catalog = RegisteredCatalog();

        var result = RuleSetRuntimeRegistrationService.Register(new RuleSetRuntimeRegistrationRequest(catalog, [new WerewolfReferenceRuntime(), new WerewolfReferenceRuntime()]));

        Assert.Empty(result.RegisteredRuntimes);
        Assert.Equal(2, result.RejectedRuntimes.Count);
        Assert.All(result.RejectedRuntimes, rejection => Assert.Equal(RuleSetRuntimeRegistrationErrorCode.DuplicateRuntime, rejection.Code));
    }

    [Fact]
    public void RejectsRuntimeWithUndeclaredOperation()
    {
        var catalog = RegisteredCatalog();
        var baseRuntime = new WerewolfReferenceRuntime();
        var runtime = new TestRuntime(baseRuntime.Metadata with
        {
            Operations =
            [
                .. baseRuntime.Metadata.Operations,
                new RuleSetOperationDescriptor("combat.roll-initiative", "combat", RuleSetOperationStatus.Enabled)
            ]
        });

        var result = RuleSetRuntimeRegistrationService.Register(new RuleSetRuntimeRegistrationRequest(catalog, [runtime]));

        var rejection = Assert.Single(result.RejectedRuntimes);
        Assert.Equal(RuleSetRuntimeRegistrationErrorCode.UndeclaredOperation, rejection.Code);
    }

    [Fact]
    public void RejectsRuntimeThatEnablesDisabledOperation()
    {
        var catalog = RegisteredCatalog();
        var baseRuntime = new WerewolfReferenceRuntime();
        var runtime = new TestRuntime(baseRuntime.Metadata with
        {
            Operations =
            [
                new RuleSetOperationDescriptor(WerewolfReferenceRuntime.CreateCharacterOperation, "character-creation", RuleSetOperationStatus.Enabled),
                new RuleSetOperationDescriptor(WerewolfReferenceRuntime.PurchaseAdditionalGiftOperation, "additional-gift-purchase", RuleSetOperationStatus.Enabled),
                new RuleSetOperationDescriptor(WerewolfReferenceRuntime.ExecuteGiftEffectOperation, "runtime-gift-execution", RuleSetOperationStatus.Disabled)
            ]
        });

        var result = RuleSetRuntimeRegistrationService.Register(new RuleSetRuntimeRegistrationRequest(catalog, [runtime]));

        var rejection = Assert.Single(result.RejectedRuntimes);
        Assert.Equal(RuleSetRuntimeRegistrationErrorCode.DisabledOperationMismatch, rejection.Code);
    }

    [Fact]
    public void LookupIsDeterministicAndImmutable()
    {
        var registry = RegisteredRuntimeRegistry();

        var first = registry.RegisteredRuntimes;
        var second = registry.RegisteredRuntimes;

        Assert.NotSame(first, second);
        Assert.Equal(Format(first), Format(second));
        Assert.Equal(
            [
                WerewolfReferenceRuntime.AllocateAbilitiesOperation,
                WerewolfReferenceRuntime.AllocateAttributesOperation,
                WerewolfReferenceRuntime.CreateCharacterOperation,
                WerewolfReferenceRuntime.PurchaseAdditionalGiftOperation,
                WerewolfReferenceRuntime.SelectAbilityPrioritiesOperation,
                WerewolfReferenceRuntime.SelectAttributePrioritiesOperation,
                WerewolfReferenceRuntime.SelectAuspiceOperation,
                WerewolfReferenceRuntime.SelectAuspiceGiftOperation,
                WerewolfReferenceRuntime.SelectMetisDeformityOperation,
                WerewolfReferenceRuntime.SelectRaceOperation,
                WerewolfReferenceRuntime.SelectRaceGiftOperation,
                WerewolfReferenceRuntime.SelectTribeOperation,
                WerewolfReferenceRuntime.SelectTribeGiftOperation,
                WerewolfReferenceRuntime.ExecuteGiftEffectOperation
            ],
            first[0].Operations.Select(operation => operation.OperationKey).ToArray());
    }

    [Fact]
    public void DisabledAndUndeclaredOperationsCannotBeInvoked()
    {
        var registry = RegisteredRuntimeRegistry();

        var disabled = registry.Execute(Request(WerewolfReferenceRuntime.PurchaseAdditionalGiftOperation));
        var undeclared = registry.Execute(Request("combat.roll-initiative"));

        Assert.Equal(RuleSetOperationFailureCode.OperationDisabled, disabled.FailureCode);
        Assert.Equal(RuleSetOperationFailureCode.OperationUndeclared, undeclared.FailureCode);
    }

    [Fact]
    public void EnabledOperationInitializesDraftThroughRuntime()
    {
        var registry = RegisteredRuntimeRegistry(new TestIdentitySource("runtime-draft-001"));

        var result = registry.Execute(Request(WerewolfReferenceRuntime.CreateCharacterOperation));

        Assert.True(result.Succeeded);
        Assert.Null(result.FailureCode);
        Assert.Equal("runtime-draft-001", result.Outputs["draftId"]);
        Assert.Equal("Initialized", result.Outputs["draftStatus"]);
        Assert.Contains("select-race", result.Outputs["nextSteps"], StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeRejectsMissingCreateCharacterRequestId()
    {
        var registry = RegisteredRuntimeRegistry(new TestIdentitySource("unused"));

        var result = registry.Execute(new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            WerewolfReferenceRuntime.CreateCharacterOperation,
            new Dictionary<string, string>(StringComparer.Ordinal)));

        Assert.False(result.Succeeded);
        Assert.Equal(RuleSetOperationFailureCode.InvalidRequest, result.FailureCode);
    }

    [Fact]
    public void RuntimeRegistrationDoesNotMutateFilesystemOrLoadImplicitly()
    {
        using var root = TemporaryDirectory.Create();
        var before = Snapshot(root.Path);

        _ = RuleSetRuntimeRegistrationService.Register(new RuleSetRuntimeRegistrationRequest(RegisteredCatalog(), [new WerewolfReferenceRuntime()]));

        Assert.Equal(before, Snapshot(root.Path));
    }

    [Fact]
    public void RuntimeBoundaryDoesNotRequireForbiddenArchitectureDependencies()
    {
        var files = Directory.EnumerateFiles(Path.Combine(FindRepositoryRoot(), "src", "Chronicle.RuleSets.Abstractions"), "*.csproj", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(Path.Combine(FindRepositoryRoot(), "rule-sets", "Chronicle.RuleSets.Werewolf"), "*.csproj", SearchOption.AllDirectories))
            .Select(File.ReadAllText);
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

        foreach (var token in forbidden)
        {
            Assert.DoesNotContain(files, text => text.Contains(token, StringComparison.Ordinal));
        }
    }

    private static RuleSetOperationRequest Request(string operationKey)
    {
        return new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            operationKey,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = "request-001"
            });
    }

    private static string Format(IEnumerable<RegisteredRuleSetRuntimeDescriptor> descriptors)
    {
        return string.Join(
            Environment.NewLine,
            descriptors.Select(descriptor =>
                $"{descriptor.Identity.PackageId}|{descriptor.Identity.PackageVersion}|{string.Join(",", descriptor.Operations.Select(operation => $"{operation.OperationKey}:{operation.Status}"))}"));
    }

    private static string Format(IEnumerable<RuleSetRuntimeFinding> findings)
    {
        return string.Join(
            Environment.NewLine,
            findings.Select(finding => $"{finding.Severity}|{finding.Code}|{finding.Message}"));
    }

    private static RuleSetRuntimeRegistry RegisteredRuntimeRegistry()
    {
        return RuleSetRuntimeRegistrationService.Register(new RuleSetRuntimeRegistrationRequest(RegisteredCatalog(), [new WerewolfReferenceRuntime()])).Registry;
    }

    private static RuleSetRuntimeRegistry RegisteredRuntimeRegistry(TestIdentitySource identitySource)
    {
        return RuleSetRuntimeRegistrationService.Register(new RuleSetRuntimeRegistrationRequest(RegisteredCatalog(), [new WerewolfReferenceRuntime(identitySource)])).Registry;
    }

    private static RuleSetPackageCatalog RegisteredCatalog()
    {
        var discovery = RuleSetPackageSourceDiscoveryService.Discover(new RuleSetPackageSourceDiscoveryRequest([RuleSetsRoot()]));
        var registration = RuleSetPackageRegistrationService.Register(new RuleSetPackageRegistrationRequest(discovery.ValidatedPackages, 1));
        return registration.Catalog;
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

    private sealed class TestRuntime(RuleSetRuntimeMetadata metadata) : IRuleSetRuntime
    {
        public RuleSetRuntimeMetadata Metadata { get; } = metadata;

        public RuleSetOperationResult Execute(RuleSetOperationRequest request)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.OperationNotImplemented,
                [],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }
    }

    private sealed class TestIdentitySource(string identity) : IWerewolfCharacterDraftIdentitySource
    {
        public WerewolfCharacterDraftIdentity CreateDraftIdentity(WerewolfCreateCharacterRequest request)
        {
            return new WerewolfCharacterDraftIdentity(identity);
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
            return new TemporaryDirectory(Directory.CreateTempSubdirectory("chronicle-runtime-").FullName);
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
