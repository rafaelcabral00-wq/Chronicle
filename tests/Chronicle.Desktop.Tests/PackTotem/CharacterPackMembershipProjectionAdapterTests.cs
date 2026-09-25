using Chronicle.Application.PackTotem;
using Chronicle.Application.Persistence;
using Chronicle.Desktop.PackTotem;
using Chronicle.Domain.PackTotem;
using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf;
using Xunit;

namespace Chronicle.Desktop.Tests.PackTotem;

public sealed class CharacterPackMembershipProjectionAdapterTests
{
    private static readonly string ValidCharacterId = "werewolf-draft-abc123";
    private static readonly string ValidCharacterId2 = "werewolf-draft-def456";
    private static readonly string ValidCharacterId3 = "werewolf-draft-ghi789";
    private static readonly string InvalidCharacterId = "invalid-character-id";

    [Fact]
    public async Task MemberReturnsPackIdPackNameLeaderId()
    {
        var harness = new InMemoryProjectionHarness();
        var adapter = harness.BuildProjectionAdapter();
        await harness.CreatePackAsync("pack-iron-wolves", "Iron Wolves");
        await harness.Orchestrator.AddMemberAsync(new AddMemberRequest("pack-iron-wolves", ValidCharacterId));

        var result = await adapter.QueryAsync(
            harness.Registry,
            new QueryCharacterPackMembershipRequest(ValidCharacterId, "req-projection-1"));

        Assert.True(result.Success);
        Assert.Equal(CharacterPackMembershipProjectionOutcome.MemberFound, result.Outcome);
        Assert.Equal("pack-iron-wolves", result.PackId);
        Assert.Equal("Iron Wolves", result.PackName);
        Assert.Null(result.LeaderId);
        Assert.True(result.IsMember);
    }

    [Fact]
    public async Task MemberWithLeaderReturnsLeaderId()
    {
        var harness = new InMemoryProjectionHarness();
        var adapter = harness.BuildProjectionAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");
        await harness.Orchestrator.AddMemberAsync(new AddMemberRequest("pack-1", ValidCharacterId));
        await harness.Orchestrator.AddMemberAsync(new AddMemberRequest("pack-1", ValidCharacterId2));
        await harness.Orchestrator.SetLeaderAsync(new SetLeaderRequest("pack-1", ValidCharacterId2));

        var result = await adapter.QueryAsync(
            harness.Registry,
            new QueryCharacterPackMembershipRequest(ValidCharacterId, "req-projection-2"));

        Assert.True(result.Success);
        Assert.Equal(CharacterPackMembershipProjectionOutcome.MemberFound, result.Outcome);
        Assert.Equal("pack-1", result.PackId);
        Assert.Equal("Pack One", result.PackName);
        Assert.Equal(ValidCharacterId2, result.LeaderId);
        Assert.True(result.IsMember);
    }

    [Fact]
    public async Task NonMemberReturnsNotAMember()
    {
        var harness = new InMemoryProjectionHarness();
        var adapter = harness.BuildProjectionAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");
        await harness.Orchestrator.AddMemberAsync(new AddMemberRequest("pack-1", ValidCharacterId));

        var result = await adapter.QueryAsync(
            harness.Registry,
            new QueryCharacterPackMembershipRequest(ValidCharacterId2, "req-projection-3"));

        Assert.True(result.Success);
        Assert.Equal(CharacterPackMembershipProjectionOutcome.NotAMember, result.Outcome);
        Assert.Null(result.PackId);
        Assert.Null(result.PackName);
        Assert.Null(result.LeaderId);
        Assert.False(result.IsMember);
    }

    [Fact]
    public async Task MultiplePacksScannedWithoutFalsePositives()
    {
        var harness = new InMemoryProjectionHarness();
        var adapter = harness.BuildProjectionAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");
        await harness.CreatePackAsync("pack-2", "Pack Two");
        await harness.Orchestrator.AddMemberAsync(new AddMemberRequest("pack-1", ValidCharacterId));
        await harness.Orchestrator.AddMemberAsync(new AddMemberRequest("pack-2", ValidCharacterId2));

        var result1 = await adapter.QueryAsync(
            harness.Registry,
            new QueryCharacterPackMembershipRequest(ValidCharacterId, "req-projection-4"));

        var result2 = await adapter.QueryAsync(
            harness.Registry,
            new QueryCharacterPackMembershipRequest(ValidCharacterId2, "req-projection-5"));

        var result3 = await adapter.QueryAsync(
            harness.Registry,
            new QueryCharacterPackMembershipRequest(ValidCharacterId3, "req-projection-6"));

        Assert.True(result1.Success);
        Assert.Equal(CharacterPackMembershipProjectionOutcome.MemberFound, result1.Outcome);
        Assert.Equal("pack-1", result1.PackId);

        Assert.True(result2.Success);
        Assert.Equal(CharacterPackMembershipProjectionOutcome.MemberFound, result2.Outcome);
        Assert.Equal("pack-2", result2.PackId);

        Assert.True(result3.Success);
        Assert.Equal(CharacterPackMembershipProjectionOutcome.NotAMember, result3.Outcome);
    }

    [Fact]
    public async Task InvalidCharacterIdentityIsRejected()
    {
        var harness = new InMemoryProjectionHarness();
        var adapter = harness.BuildProjectionAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");

        var result = await adapter.QueryAsync(
            harness.Registry,
            new QueryCharacterPackMembershipRequest(InvalidCharacterId, "req-projection-7"));

        Assert.False(result.Success);
        Assert.Equal(CharacterPackMembershipProjectionOutcome.InvalidCharacterIdentity, result.Outcome);
        Assert.Equal($"Character identity '{InvalidCharacterId}' is invalid.", result.FailureReason);
    }

    [Fact]
    public async Task EmptyCharacterIdentityIsRejected()
    {
        var harness = new InMemoryProjectionHarness();
        var adapter = harness.BuildProjectionAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");

        var result = await adapter.QueryAsync(
            harness.Registry,
            new QueryCharacterPackMembershipRequest(string.Empty, "req-projection-8"));

        Assert.False(result.Success);
        Assert.Equal(CharacterPackMembershipProjectionOutcome.InvalidCharacterIdentity, result.Outcome);
    }

    [Fact]
    public async Task WhitespaceCharacterIdentityIsRejected()
    {
        var harness = new InMemoryProjectionHarness();
        var adapter = harness.BuildProjectionAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");

        var result = await adapter.QueryAsync(
            harness.Registry,
            new QueryCharacterPackMembershipRequest("   ", "req-projection-9"));

        Assert.False(result.Success);
        Assert.Equal(CharacterPackMembershipProjectionOutcome.InvalidCharacterIdentity, result.Outcome);
    }

    [Fact]
    public async Task ReadOperationDoesNotMutateOrPersist()
    {
        var harness = new InMemoryProjectionHarness();
        var adapter = harness.BuildProjectionAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");
        await harness.Orchestrator.AddMemberAsync(new AddMemberRequest("pack-1", ValidCharacterId));
        var versionBefore = harness.GetVersion("pack-1");

        var result = await adapter.QueryAsync(
            harness.Registry,
            new QueryCharacterPackMembershipRequest(ValidCharacterId, "req-projection-10"));

        Assert.True(result.Success);
        Assert.Equal(versionBefore, harness.GetVersion("pack-1"));
        Assert.Contains(ValidCharacterId, harness.GetState("pack-1").Members);
    }

    [Fact]
    public async Task ExistingMembershipMutationTestsRemainGreen()
    {
        var harness = new InMemoryProjectionHarness();
        var adapter = harness.BuildProjectionAdapter();
        var mutationAdapter = harness.BuildMutationAdapter();
        await harness.CreatePackAsync("pack-1", "Pack One");

        var add = await mutationAdapter.AddMemberAsync(
            harness.Registry,
            new IntegrateCharacterPackMembershipRequest(
                PackId: "pack-1",
                CharacterId: ValidCharacterId,
                OperationKey: "execute-rite",
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                RequestId: "req-mutation-1"));

        Assert.True(add.ChronicleMutationSucceeded);

        var projection = await adapter.QueryAsync(
            harness.Registry,
            new QueryCharacterPackMembershipRequest(ValidCharacterId, "req-projection-11"));

        Assert.True(projection.Success);
        Assert.Equal(CharacterPackMembershipProjectionOutcome.MemberFound, projection.Outcome);
        Assert.Equal("pack-1", projection.PackId);

        var remove = await mutationAdapter.RemoveMemberAsync(
            harness.Registry,
            new IntegrateCharacterPackMembershipRequest(
                PackId: "pack-1",
                CharacterId: ValidCharacterId,
                OperationKey: "execute-rite",
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                RequestId: "req-mutation-2"));

        Assert.True(remove.ChronicleMutationSucceeded);

        var projectionAfterRemove = await adapter.QueryAsync(
            harness.Registry,
            new QueryCharacterPackMembershipRequest(ValidCharacterId, "req-projection-12"));

        Assert.True(projectionAfterRemove.Success);
        Assert.Equal(CharacterPackMembershipProjectionOutcome.NotAMember, projectionAfterRemove.Outcome);
    }

    private sealed class InMemoryProjectionHarness
    {
        private static readonly DateTimeOffset Now = new(2026, 9, 2, 0, 0, 0, TimeSpan.Zero);
        private readonly Dictionary<string, Document> documents = new();
        private readonly Dictionary<string, Guid> ids = new();

        public InMemoryProjectionHarness()
        {
            Registry = BuildRegistry();
            Store = new AggregateStore(new InMemoryDocumentRepository(documents, ids));
            Orchestrator = new PackTotemOrchestrator(Store);
        }

        public RuleSetRuntimeRegistry Registry { get; }

        public AggregateStore Store { get; }

        public PackTotemOrchestrator Orchestrator { get; }

        public CharacterPackMembershipProjectionAdapter BuildProjectionAdapter()
        {
            return new CharacterPackMembershipProjectionAdapter(Orchestrator);
        }

        public CharacterPackMembershipIntegrationAdapter BuildMutationAdapter()
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