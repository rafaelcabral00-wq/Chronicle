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

public sealed class CharacterPackMembershipIntegrationAdapterTests
{
    private static readonly string ValidCharacterId = "werewolf-draft-abc123";
    private static readonly string ValidCharacterId2 = "werewolf-draft-def456";
    private static readonly string InvalidCharacterId = "invalid-character-id";

    [Fact]
    public async Task CompletedCharacterCanBeAddedToPack()
    {
        var harness = new InMemoryCharacterPackHarness();
        var adapter = harness.BuildIntegrationAdapter();
        await harness.CreatePackAsync("pack-iron-wolves", "Iron Wolves");

        var result = await adapter.AddMemberAsync(
            harness.Registry,
            new IntegrateCharacterPackMembershipRequest(
                PackId: "pack-iron-wolves",
                CharacterId: ValidCharacterId,
                OperationKey: "execute-rite",
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                RequestId: "req-membership-1"));

        Assert.True(result.ChronicleMutationSucceeded);
        Assert.Equal(CharacterPackMembershipIntegrationOutcome.MemberAdded, result.Outcome);
        Assert.NotNull(result.AggregateResult);
        Assert.True(result.AggregateResult!.Succeeded);

        var state = harness.GetState("pack-iron-wolves");
        Assert.Single(state.Members);
        Assert.Contains(ValidCharacterId, state.Members);
    }

    [Fact]
    public async Task PackContainsRealCharacterIdentity()
    {
        var harness = new InMemoryCharacterPackHarness();
        var adapter = harness.BuildIntegrationAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");

        var result = await adapter.AddMemberAsync(
            harness.Registry,
            new IntegrateCharacterPackMembershipRequest(
                PackId: "pack-1",
                CharacterId: ValidCharacterId,
                OperationKey: "execute-rite",
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                RequestId: "req-membership-2"));

        Assert.True(result.ChronicleMutationSucceeded);

        var state = harness.GetState("pack-1");
        Assert.Contains(ValidCharacterId, state.Members);
        Assert.Equal(ValidCharacterId, state.Members[0]);
    }

    [Fact]
    public async Task SameCharacterCannotBeAddedTwice()
    {
        var harness = new InMemoryCharacterPackHarness();
        var adapter = harness.BuildIntegrationAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");

        var first = await adapter.AddMemberAsync(
            harness.Registry,
            new IntegrateCharacterPackMembershipRequest(
                PackId: "pack-1",
                CharacterId: ValidCharacterId,
                OperationKey: "execute-rite",
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                RequestId: "req-membership-3"));

        Assert.True(first.ChronicleMutationSucceeded);

        var second = await adapter.AddMemberAsync(
            harness.Registry,
            new IntegrateCharacterPackMembershipRequest(
                PackId: "pack-1",
                CharacterId: ValidCharacterId,
                OperationKey: "execute-rite",
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                RequestId: "req-membership-4"));

        Assert.False(second.ChronicleMutationSucceeded);
        Assert.Equal(CharacterPackMembershipIntegrationOutcome.AggregateInvariantViolated, second.Outcome);
        Assert.NotNull(second.AggregateResult);
        Assert.False(second.AggregateResult!.Succeeded);

        var state = harness.GetState("pack-1");
        Assert.Single(state.Members);
    }

    [Fact]
    public async Task CharacterCanLeavePack()
    {
        var harness = new InMemoryCharacterPackHarness();
        var adapter = harness.BuildIntegrationAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");

        var add = await adapter.AddMemberAsync(
            harness.Registry,
            new IntegrateCharacterPackMembershipRequest(
                PackId: "pack-1",
                CharacterId: ValidCharacterId,
                OperationKey: "execute-rite",
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                RequestId: "req-membership-5"));

        Assert.True(add.ChronicleMutationSucceeded);

        var remove = await adapter.RemoveMemberAsync(
            harness.Registry,
            new IntegrateCharacterPackMembershipRequest(
                PackId: "pack-1",
                CharacterId: ValidCharacterId,
                OperationKey: "execute-rite",
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                RequestId: "req-membership-6"));

        Assert.True(remove.ChronicleMutationSucceeded);
        Assert.Equal(CharacterPackMembershipIntegrationOutcome.MemberRemoved, remove.Outcome);
        Assert.NotNull(remove.AggregateResult);
        Assert.True(remove.AggregateResult!.Succeeded);

        var state = harness.GetState("pack-1");
        Assert.Empty(state.Members);
    }

    [Fact]
    public async Task MissingPackIsRejected()
    {
        var harness = new InMemoryCharacterPackHarness();
        var adapter = harness.BuildIntegrationAdapter();

        var result = await adapter.AddMemberAsync(
            harness.Registry,
            new IntegrateCharacterPackMembershipRequest(
                PackId: "missing-pack",
                CharacterId: ValidCharacterId,
                OperationKey: "execute-rite",
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                RequestId: "req-membership-7"));

        Assert.False(result.ChronicleMutationSucceeded);
        Assert.Equal(CharacterPackMembershipIntegrationOutcome.PackNotFound, result.Outcome);
        Assert.NotNull(result.AggregateResult);
        Assert.False(result.AggregateResult!.Succeeded);
        Assert.False(harness.PackExists("missing-pack"));
    }

    [Fact]
    public async Task InvalidCharacterIdentityIsRejected()
    {
        var harness = new InMemoryCharacterPackHarness();
        var adapter = harness.BuildIntegrationAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");

        var result = await adapter.AddMemberAsync(
            harness.Registry,
            new IntegrateCharacterPackMembershipRequest(
                PackId: "pack-1",
                CharacterId: InvalidCharacterId,
                OperationKey: "execute-rite",
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                RequestId: "req-membership-8"));

        Assert.False(result.ChronicleMutationSucceeded);
        Assert.Equal(CharacterPackMembershipIntegrationOutcome.InvalidCharacterIdentity, result.Outcome);
        Assert.NotNull(result.AggregateResult);
        Assert.False(result.AggregateResult!.Succeeded);
    }

    [Fact]
    public async Task EmptyCharacterIdentityIsRejected()
    {
        var harness = new InMemoryCharacterPackHarness();
        var adapter = harness.BuildIntegrationAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");

        var result = await adapter.AddMemberAsync(
            harness.Registry,
            new IntegrateCharacterPackMembershipRequest(
                PackId: "pack-1",
                CharacterId: string.Empty,
                OperationKey: "execute-rite",
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                RequestId: "req-membership-9"));

        Assert.False(result.ChronicleMutationSucceeded);
        Assert.Equal(CharacterPackMembershipIntegrationOutcome.InvalidCharacterIdentity, result.Outcome);
    }

    [Fact]
    public async Task WhitespaceCharacterIdentityIsRejected()
    {
        var harness = new InMemoryCharacterPackHarness();
        var adapter = harness.BuildIntegrationAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");

        var result = await adapter.AddMemberAsync(
            harness.Registry,
            new IntegrateCharacterPackMembershipRequest(
                PackId: "pack-1",
                CharacterId: "   ",
                OperationKey: "execute-rite",
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                RequestId: "req-membership-10"));

        Assert.False(result.ChronicleMutationSucceeded);
        Assert.Equal(CharacterPackMembershipIntegrationOutcome.InvalidCharacterIdentity, result.Outcome);
    }

    [Fact]
    public async Task MultipleCharactersCanBeAddedToSamePack()
    {
        var harness = new InMemoryCharacterPackHarness();
        var adapter = harness.BuildIntegrationAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");

        var add1 = await adapter.AddMemberAsync(
            harness.Registry,
            new IntegrateCharacterPackMembershipRequest(
                PackId: "pack-1",
                CharacterId: ValidCharacterId,
                OperationKey: "execute-rite",
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                RequestId: "req-membership-11"));

        var add2 = await adapter.AddMemberAsync(
            harness.Registry,
            new IntegrateCharacterPackMembershipRequest(
                PackId: "pack-1",
                CharacterId: ValidCharacterId2,
                OperationKey: "execute-rite",
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                RequestId: "req-membership-12"));

        Assert.True(add1.ChronicleMutationSucceeded);
        Assert.True(add2.ChronicleMutationSucceeded);

        var state = harness.GetState("pack-1");
        Assert.Equal(2, state.Members.Count);
        Assert.Contains(ValidCharacterId, state.Members);
        Assert.Contains(ValidCharacterId2, state.Members);
    }

    [Fact]
    public async Task RemoveNonMemberIsRejected()
    {
        var harness = new InMemoryCharacterPackHarness();
        var adapter = harness.BuildIntegrationAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");

        var result = await adapter.RemoveMemberAsync(
            harness.Registry,
            new IntegrateCharacterPackMembershipRequest(
                PackId: "pack-1",
                CharacterId: ValidCharacterId,
                OperationKey: "execute-rite",
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                RequestId: "req-membership-13"));

        Assert.False(result.ChronicleMutationSucceeded);
        Assert.Equal(CharacterPackMembershipIntegrationOutcome.AggregateInvariantViolated, result.Outcome);
    }

    [Fact]
    public async Task IntegrationDoesNotAutomaticallyBindTotem()
    {
        var harness = new InMemoryCharacterPackHarness();
        var adapter = harness.BuildIntegrationAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");

        var add = await adapter.AddMemberAsync(
            harness.Registry,
            new IntegrateCharacterPackMembershipRequest(
                PackId: "pack-1",
                CharacterId: ValidCharacterId,
                OperationKey: "execute-rite",
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                RequestId: "req-membership-14"));

        Assert.True(add.ChronicleMutationSucceeded);

        var state = harness.GetState("pack-1");
        Assert.Null(state.TotemId);
        Assert.Equal(PackTotemLinkState.Unbound, state.LinkState);
    }

    private sealed class InMemoryCharacterPackHarness
    {
        private static readonly DateTimeOffset Now = new(2026, 9, 2, 0, 0, 0, TimeSpan.Zero);
        private readonly Dictionary<string, Document> documents = new();
        private readonly Dictionary<string, Guid> ids = new();

        public InMemoryCharacterPackHarness()
        {
            Registry = BuildRegistry();
            Store = new AggregateStore(new InMemoryDocumentRepository(documents, ids));
            Orchestrator = new PackTotemOrchestrator(Store);
        }

        public RuleSetRuntimeRegistry Registry { get; }

        public AggregateStore Store { get; }

        public PackTotemOrchestrator Orchestrator { get; }

        public CharacterPackMembershipIntegrationAdapter BuildIntegrationAdapter()
        {
            return new CharacterPackMembershipIntegrationAdapter(Orchestrator);
        }

        public async Task CreatePackAsync(string packId, string packName)
        {
            var result = await Orchestrator.CreatePackAsync(new CreatePackRequest(packId, packName, Now));
            Assert.True(result.Succeeded, $"Failed to seed pack '{packId}': {result.FailureReason}");
        }

        public long GetVersion(string packId) =>
            documents.TryGetValue(packId, out var document) ? document.Version : 0;

        public bool PackExists(string packId) => documents.ContainsKey(packId);

        public PackTotemState GetState(string packId) =>
            PackTotemSerializer.Deserialize(documents[packId].PayloadJson);

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

            public Task<IReadOnlyList<Document>> EnumerateAsync(CancellationToken cancellationToken = default)
            {
                return Task.FromResult<IReadOnlyList<Document>>(documents.Values.ToArray());
            }
        }
    }
}