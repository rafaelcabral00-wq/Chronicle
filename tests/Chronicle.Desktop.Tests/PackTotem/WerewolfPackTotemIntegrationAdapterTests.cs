using Chronicle.Application.PackTotem;
using Chronicle.Application.Persistence;
using Chronicle.Desktop.PackTotem;
using Chronicle.Domain.PackTotem;
using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.Desktop.Tests.PackTotem;

public sealed class WerewolfPackTotemIntegrationAdapterTests
{
    private static readonly IReadOnlyList<int> SuccessDice = [7, 8, 9];
    private static readonly IReadOnlyList<int> ZeroSuccessDice = [1, 1, 2];
    private static readonly IReadOnlyList<int> InvalidDice = [0, 0, 0];
    private static readonly IReadOnlyList<string> NoImprovements = [];
    private static readonly IReadOnlyList<string> TwoImprovements = ["communal-senses", "pack-speech"];

    [Fact]
    public async Task SuccessfulRiteBindsRealChronicleTotemAndPersists()
    {
        var harness = new InMemoryPackTotemHarness();
        var adapter = harness.BuildAdapter();
        await harness.CreatePackAsync("pack-iron-wolves", "Iron Wolves");
        var aggregateId = harness.GetAggregateId("pack-iron-wolves");

        var result = await adapter.IntegrateAsync(
            harness.Registry,
            new IntegratePackTotemBindingRequest(
                PackIdAggregateId: aggregateId,
                PackId: "pack-iron-wolves",
                TotemId: "falcon-totem",
                TotemRating: 4,
                TotemAggregation: 7,
                InitialImprovementPurchases: TwoImprovements,
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                OperationKey: WerewolfReferenceRuntime.ExecuteRiteOperation,
                ExpectedRiteKey: WerewolfRiteIdentifiers.Totem,
                DiceValues: SuccessDice,
                HasTargetPiece: false,
                RequestId: "req-e4-success"));

        Assert.True(result.WerewolfSucceeded);
        Assert.True(result.ChronicleMutationSucceeded);
        Assert.Equal(PackTotemIntegrationOutcome.Bound, result.Outcome);
        Assert.Equal(PackTotemBoundaryValidationKind.BoundarySignalReceived, result.BoundaryValidation.Kind);
        Assert.NotNull(result.AggregateResult);
        Assert.True(result.AggregateResult!.Succeeded);
        Assert.Equal(PackTotemLinkState.Bound, result.AggregateResult.LinkState);

        var state = harness.GetState("pack-iron-wolves");
        Assert.Equal("falcon-totem", state.TotemId);
        Assert.Equal(4, state.TotemRating);
        Assert.Equal(PackTotemLinkState.Bound, state.LinkState);
        Assert.Contains("communal-senses", state.TotemImprovementPurchases);
        Assert.Contains("pack-speech", state.TotemImprovementPurchases);
    }

    [Fact]
    public async Task PlaceholderTotemIdAndPackIdAreNeverPersisted()
    {
        var harness = new InMemoryPackTotemHarness();
        var adapter = harness.BuildAdapter();
        await harness.CreatePackAsync("pack-iron-wolves", "Iron Wolves");
        var aggregateId = harness.GetAggregateId("pack-iron-wolves");

        var result = await adapter.IntegrateAsync(
            harness.Registry,
            new IntegratePackTotemBindingRequest(
                PackIdAggregateId: aggregateId,
                PackId: "pack-iron-wolves",
                TotemId: "real-falcon-totem",
                TotemRating: 3,
                TotemAggregation: 5,
                InitialImprovementPurchases: NoImprovements,
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                OperationKey: WerewolfReferenceRuntime.ExecuteRiteOperation,
                ExpectedRiteKey: WerewolfRiteIdentifiers.Totem,
                DiceValues: SuccessDice,
                HasTargetPiece: false,
                RequestId: "req-e4-no-placeholder"));

        Assert.True(result.ChronicleMutationSucceeded);
        var state = harness.GetState("pack-iron-wolves");
        Assert.Equal("real-falcon-totem", state.TotemId);
        Assert.DoesNotContain("TotemId from Chronicle", state.TotemId);
        Assert.Equal("pack-iron-wolves", state.PackId);
        Assert.DoesNotContain("PackId from Chronicle", state.PackId);
    }

    [Fact]
    public async Task WeakButValidDiceStillTriggerBoundarySignalAndBinding()
    {
        var harness = new InMemoryPackTotemHarness();
        var adapter = harness.BuildAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");
        var aggregateId = harness.GetAggregateId("pack-1");

        var result = await adapter.IntegrateAsync(
            harness.Registry,
            new IntegratePackTotemBindingRequest(
                PackIdAggregateId: aggregateId,
                PackId: "pack-1",
                TotemId: "wolf-totem",
                TotemRating: 3,
                TotemAggregation: 2,
                InitialImprovementPurchases: NoImprovements,
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                OperationKey: WerewolfReferenceRuntime.ExecuteRiteOperation,
                ExpectedRiteKey: WerewolfRiteIdentifiers.Totem,
                DiceValues: ZeroSuccessDice,
                HasTargetPiece: false,
                RequestId: "req-e4-weak"));

        Assert.Equal(PackTotemBoundaryValidationKind.BoundarySignalReceived, result.BoundaryValidation.Kind);
        Assert.True(result.BoundaryValidation.Succeeded);
        Assert.True(result.ChronicleMutationSucceeded);
        Assert.Equal(PackTotemIntegrationOutcome.Bound, result.Outcome);

        var state = harness.GetState("pack-1");
        Assert.Equal("wolf-totem", state.TotemId);
        Assert.Equal(PackTotemLinkState.Bound, state.LinkState);
    }

    [Fact]
    public async Task InvalidDicePreventAggregateMutation()
    {
        var harness = new InMemoryPackTotemHarness();
        var adapter = harness.BuildAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");
        var aggregateId = harness.GetAggregateId("pack-1");
        var versionBefore = harness.GetVersion("pack-1");

        var result = await adapter.IntegrateAsync(
            harness.Registry,
            new IntegratePackTotemBindingRequest(
                PackIdAggregateId: aggregateId,
                PackId: "pack-1",
                TotemId: "wolf-totem",
                TotemRating: 3,
                TotemAggregation: 2,
                InitialImprovementPurchases: NoImprovements,
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                OperationKey: WerewolfReferenceRuntime.ExecuteRiteOperation,
                ExpectedRiteKey: WerewolfRiteIdentifiers.Totem,
                DiceValues: InvalidDice,
                HasTargetPiece: false,
                RequestId: "req-e4-invalid-dice"));

        Assert.False(result.WerewolfSucceeded);
        Assert.False(result.ChronicleMutationSucceeded);
        Assert.Equal(PackTotemIntegrationOutcome.BoundarySignalNotReceived, result.Outcome);
        Assert.Equal(PackTotemBoundaryValidationKind.RiteTestFailed, result.BoundaryValidation.Kind);
        Assert.Null(result.AggregateResult);
        Assert.Equal(versionBefore, harness.GetVersion("pack-1"));
        Assert.Equal(PackTotemLinkState.Unbound, harness.GetState("pack-1").LinkState);
    }

    [Fact]
    public async Task UnknownPackReportsAggregateNotFoundWithoutPersisting()
    {
        var harness = new InMemoryPackTotemHarness();
        var adapter = harness.BuildAdapter();

        var result = await adapter.IntegrateAsync(
            harness.Registry,
            new IntegratePackTotemBindingRequest(
                PackIdAggregateId: Guid.NewGuid(),
                PackId: "missing-pack",
                TotemId: "wolf-totem",
                TotemRating: 3,
                TotemAggregation: 1,
                InitialImprovementPurchases: NoImprovements,
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                OperationKey: WerewolfReferenceRuntime.ExecuteRiteOperation,
                ExpectedRiteKey: WerewolfRiteIdentifiers.Totem,
                DiceValues: SuccessDice,
                HasTargetPiece: false,
                RequestId: "req-e4-missing"));

        Assert.True(result.WerewolfSucceeded);
        Assert.False(result.ChronicleMutationSucceeded);
        Assert.Equal(PackTotemIntegrationOutcome.AggregateNotFound, result.Outcome);
        Assert.NotNull(result.AggregateResult);
        Assert.False(result.AggregateResult!.Succeeded);
    }

    [Fact]
    public async Task SecondBindingOnAlreadyBoundPackIsReportedAsInvariantViolation()
    {
        var harness = new InMemoryPackTotemHarness();
        var adapter = harness.BuildAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");
        var aggregateId = harness.GetAggregateId("pack-1");

        var first = await adapter.IntegrateAsync(
            harness.Registry,
            new IntegratePackTotemBindingRequest(
                PackIdAggregateId: aggregateId,
                PackId: "pack-1",
                TotemId: "falcon",
                TotemRating: 3,
                TotemAggregation: 5,
                InitialImprovementPurchases: NoImprovements,
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                OperationKey: WerewolfReferenceRuntime.ExecuteRiteOperation,
                ExpectedRiteKey: WerewolfRiteIdentifiers.Totem,
                DiceValues: SuccessDice,
                HasTargetPiece: false,
                RequestId: "req-e4-first"));
        Assert.True(first.ChronicleMutationSucceeded);

        var second = await adapter.IntegrateAsync(
            harness.Registry,
            new IntegratePackTotemBindingRequest(
                PackIdAggregateId: aggregateId,
                PackId: "pack-1",
                TotemId: "wolf",
                TotemRating: 4,
                TotemAggregation: 6,
                InitialImprovementPurchases: NoImprovements,
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                OperationKey: WerewolfReferenceRuntime.ExecuteRiteOperation,
                ExpectedRiteKey: WerewolfRiteIdentifiers.Totem,
                DiceValues: SuccessDice,
                HasTargetPiece: false,
                RequestId: "req-e4-second"));

        Assert.True(second.WerewolfSucceeded);
        Assert.False(second.ChronicleMutationSucceeded);
        Assert.Equal(PackTotemIntegrationOutcome.AggregateInvariantViolated, second.Outcome);
        Assert.NotNull(second.AggregateResult);
        Assert.False(second.AggregateResult!.Succeeded);

        var state = harness.GetState("pack-1");
        Assert.Equal("falcon", state.TotemId);
        Assert.Equal(PackTotemLinkState.Bound, state.LinkState);
    }

    [Fact]
    public async Task PersistenceRoundTripPreservesPackIdTotemIdAndLinkState()
    {
        var harness = new InMemoryPackTotemHarness();
        var adapter = harness.BuildAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");
        var aggregateId = harness.GetAggregateId("pack-1");

        var result = await adapter.IntegrateAsync(
            harness.Registry,
            new IntegratePackTotemBindingRequest(
                PackIdAggregateId: aggregateId,
                PackId: "pack-1",
                TotemId: "bear-totem",
                TotemRating: 5,
                TotemAggregation: 8,
                InitialImprovementPurchases: TwoImprovements,
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                OperationKey: WerewolfReferenceRuntime.ExecuteRiteOperation,
                ExpectedRiteKey: WerewolfRiteIdentifiers.Totem,
                DiceValues: SuccessDice,
                HasTargetPiece: false,
                RequestId: "req-e4-roundtrip"));

        Assert.True(result.ChronicleMutationSucceeded);
        var reloaded = harness.LoadState("pack-1", aggregateId);

        Assert.Equal("pack-1", reloaded.PackId);
        Assert.Equal("bear-totem", reloaded.TotemId);
        Assert.Equal(5, reloaded.TotemRating);
        Assert.Equal(PackTotemLinkState.Bound, reloaded.LinkState);
        Assert.Equal(2, reloaded.TotemImprovementPurchases.Count);
        Assert.Equal(TotemXpResolutionState.Unresolved, reloaded.LastTotemXpResolution);
        Assert.Equal(2, harness.GetVersion("pack-1"));
    }

    [Fact]
    public async Task SingleExecutionIsEvidencedByDeterministicBoundaryObservation()
    {
        var harness = new InMemoryPackTotemHarness();
        var adapter = harness.BuildAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");
        var aggregateId = harness.GetAggregateId("pack-1");

        var integrateRequest = new IntegratePackTotemBindingRequest(
            PackIdAggregateId: aggregateId,
            PackId: "pack-1",
            TotemId: "falcon",
            TotemRating: 3,
            TotemAggregation: 5,
            InitialImprovementPurchases: NoImprovements,
            PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
            PackageVersion: WerewolfRuleSetPackage.PackageVersion,
            OperationKey: WerewolfReferenceRuntime.ExecuteRiteOperation,
            ExpectedRiteKey: WerewolfRiteIdentifiers.Totem,
            DiceValues: SuccessDice,
            HasTargetPiece: false,
            RequestId: "req-e4-singleton");

        var first = await adapter.IntegrateAsync(harness.Registry, integrateRequest);
        var second = await adapter.IntegrateAsync(harness.Registry, integrateRequest);

        Assert.NotNull(first.BoundaryValidation.S4Observation);
        Assert.NotNull(second.BoundaryValidation.S4Observation);
        var o1 = first.BoundaryValidation.S4Observation!;
        var o2 = second.BoundaryValidation.S4Observation!;
        Assert.Equal(o1.SuccessCount, o2.SuccessCount);
        Assert.Equal(o1.InterpretationStatus, o2.InterpretationStatus);
        Assert.Equal(o1.SourceLocator, o2.SourceLocator);
        Assert.Equal(o1.Effect, o2.Effect);
    }

    [Fact]
    public async Task WrongRiteKeyShortCircuitsBeforeChronicleMutation()
    {
        var harness = new InMemoryPackTotemHarness();
        var adapter = harness.BuildAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");
        var aggregateId = harness.GetAggregateId("pack-1");
        var versionBefore = harness.GetVersion("pack-1");

        var result = await adapter.IntegrateAsync(
            harness.Registry,
            new IntegratePackTotemBindingRequest(
                PackIdAggregateId: aggregateId,
                PackId: "pack-1",
                TotemId: "falcon",
                TotemRating: 3,
                TotemAggregation: 5,
                InitialImprovementPurchases: NoImprovements,
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                OperationKey: WerewolfReferenceRuntime.ExecuteRiteOperation,
                ExpectedRiteKey: WerewolfRiteIdentifiers.Fetish,
                DiceValues: SuccessDice,
                HasTargetPiece: false,
                RequestId: "req-e4-wrong-key"));

        Assert.False(result.WerewolfSucceeded);
        Assert.False(result.ChronicleMutationSucceeded);
        Assert.Equal(PackTotemIntegrationOutcome.BoundarySignalNotReceived, result.Outcome);
        Assert.Equal(PackTotemBoundaryValidationKind.InvalidRiteKey, result.BoundaryValidation.Kind);
        Assert.Null(result.AggregateResult);
        Assert.Equal(versionBefore, harness.GetVersion("pack-1"));
    }

    private sealed class InMemoryPackTotemHarness
    {
        private static readonly DateTimeOffset Now = new(2026, 9, 2, 0, 0, 0, TimeSpan.Zero);
        private readonly Dictionary<string, Document> documents = new();
        private readonly Dictionary<string, Guid> ids = new();

        public InMemoryPackTotemHarness()
        {
            Registry = BuildRegistry();
            Store = new AggregateStore(new InMemoryDocumentRepository(documents, ids));
        }

        public RuleSetRuntimeRegistry Registry { get; }

        public AggregateStore Store { get; }

        public WerewolfPackTotemIntegrationAdapter BuildAdapter()
        {
            var orchestrator = new PackTotemOrchestrator(Store);
            var boundaryAdapter = new WerewolfPackTotemBoundaryAdapter();
            return new WerewolfPackTotemIntegrationAdapter(boundaryAdapter, orchestrator);
        }

        public async Task CreatePackAsync(string packId, string packName)
        {
            var orchestrator = new PackTotemOrchestrator(Store);
            var result = await orchestrator.CreatePackAsync(new CreatePackRequest(packId, packName, Now));
            Assert.True(result.Succeeded, $"Failed to seed pack '{packId}': {result.FailureReason}");
        }

        public Guid GetAggregateId(string packId) => ids[packId];

        public long GetVersion(string packId) => documents[packId].Version;

        public PackTotemState GetState(string packId) =>
            PackTotemSerializer.Deserialize(documents[packId].PayloadJson);

        public PackTotemState LoadState(string packId, Guid aggregateId)
        {
            var document = documents[packId];
            Assert.Equal(aggregateId, document.Id);
            return PackTotemSerializer.Deserialize(document.PayloadJson);
        }

        private static RuleSetRuntimeRegistry BuildRegistry()
        {
            var discovery = RuleSetPackageSourceDiscoveryService.Discover(new RuleSetPackageSourceDiscoveryRequest([RuleSetsRoot()]));
            var registration = RuleSetPackageRegistrationService.Register(new RuleSetPackageRegistrationRequest(discovery.ValidatedPackages, 1));
            return RuleSetRuntimeRegistrationService.Register(new RuleSetRuntimeRegistrationRequest(registration.Catalog, [new WerewolfReferenceRuntime()])).Registry;
        }

        private static string RuleSetsRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "Chronicle.sln")))
                {
                    return Path.Combine(directory.FullName, "rule-sets");
                }

                directory = directory.Parent;
            }

            throw new InvalidOperationException("Could not find repository root from test base directory.");
        }

        private sealed class InMemoryDocumentRepository : IDocumentRepository
        {
            private readonly Dictionary<string, Document> documents;
            private readonly Dictionary<string, Guid> ids;

            public InMemoryDocumentRepository(
                Dictionary<string, Document> documents,
                Dictionary<string, Guid> ids)
            {
                this.documents = documents;
                this.ids = ids;
            }

            public Task<DocumentPersistenceResult> LoadAsync(Guid id, CancellationToken cancellationToken = default)
            {
                foreach (var document in documents.Values)
                {
                    if (document.Id == id)
                    {
                        return Task.FromResult(new DocumentPersistenceResult(
                            DocumentPersistenceStatus.Succeeded, document, null));
                    }
                }

                return Task.FromResult(new DocumentPersistenceResult(
                    DocumentPersistenceStatus.NotFound, null, null));
            }

            public Task<DocumentPersistenceResult> SaveAsync(
                Document document,
                long? expectedVersion,
                CancellationToken cancellationToken = default)
            {
                var state = PackTotemSerializer.Deserialize(document.PayloadJson);
                if (documents.TryGetValue(state.PackId, out var existing))
                {
                    if (expectedVersion is null)
                    {
                        return Task.FromResult(new DocumentPersistenceResult(
                            DocumentPersistenceStatus.ConcurrencyConflict, null, "Already exists."));
                    }

                    if (expectedVersion.Value != existing.Version)
                    {
                        return Task.FromResult(new DocumentPersistenceResult(
                            DocumentPersistenceStatus.ConcurrencyConflict, null, "Version mismatch."));
                    }

                    var updated = document with { Version = existing.Version + 1 };
                    documents[state.PackId] = updated;
                    return Task.FromResult(new DocumentPersistenceResult(
                        DocumentPersistenceStatus.Succeeded, updated, null));
                }
                else
                {
                    if (expectedVersion is not null)
                    {
                        return Task.FromResult(new DocumentPersistenceResult(
                            DocumentPersistenceStatus.ConcurrencyConflict, null, "Expected version but absent."));
                    }

                    var created = document with { Version = 1 };
                    documents[state.PackId] = created;
                    ids[state.PackId] = created.Id;
                    return Task.FromResult(new DocumentPersistenceResult(
                        DocumentPersistenceStatus.Succeeded, created, null));
                }
            }

            public Task<DocumentPersistenceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            {
                foreach (var (packId, document) in documents.ToArray())
                {
                    if (document.Id == id)
                    {
                        documents.Remove(packId);
                        ids.Remove(packId);
                        return Task.FromResult(new DocumentPersistenceResult(
                            DocumentPersistenceStatus.Succeeded, null, null));
                    }
                }

                return Task.FromResult(new DocumentPersistenceResult(
                    DocumentPersistenceStatus.NotFound, null, null));
            }
        }
    }
}
