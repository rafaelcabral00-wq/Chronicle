using System.Text.Json;
using Chronicle.Application.Persistence;
using Chronicle.Domain;
using Xunit;

namespace Chronicle.Application.Tests;

public sealed class AggregateStoreTests
{
    private sealed class SampleCreatedEvent : IDomainEvent
    {
        public string Name { get; }
        public SampleCreatedEvent(string name) { Name = name; }
    }

    private sealed class SampleRenamedEvent : IDomainEvent
    {
        public string NewName { get; }
        public SampleRenamedEvent(string newName) { NewName = newName; }
    }

    private sealed class SampleAggregate : AggregateRoot
    {
        public string Name { get; private set; } = string.Empty;

        public SampleAggregate()
        {
        }

        internal SampleAggregate(Guid id) : base(id)
        {
        }

        public static SampleAggregate CreateNew(string name)
        {
            var aggregate = new SampleAggregate();
            aggregate.Name = name;
            aggregate.RecordEvent(new SampleCreatedEvent(name));
            return aggregate;
        }

        public void Rename(string newName)
        {
            Name = newName;
            RecordEvent(new SampleRenamedEvent(newName));
        }

        internal static SampleAggregate Rehydrate(Guid id, string name)
        {
            var aggregate = new SampleAggregate(id);
            aggregate.Name = name;
            return aggregate;
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private static Document SerializeSample(SampleAggregate aggregate)
    {
        var payload = JsonSerializer.Serialize(new { aggregate.Id, Name = aggregate.Name }, JsonOptions);
        return new Document(aggregate.Id, "sample-aggregate", payload, 0);
    }

    private static (SampleAggregate aggregate, long version) DeserializeSample(Document document)
    {
        var dto = JsonSerializer.Deserialize<Dto>(document.PayloadJson, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize sample aggregate.");
        return (SampleAggregate.Rehydrate(dto.Id, dto.Name), document.Version);
    }

    private sealed record Dto(Guid Id, string Name);

    private sealed class InMemoryDocumentRepository : IDocumentRepository
    {
        public readonly Dictionary<Guid, Document> Store = new();
        public int LoadCount;
        public int SaveCount;
        public int DeleteCount;

        public Task<DocumentPersistenceResult> LoadAsync(Guid id, CancellationToken cancellationToken = default)
        {
            LoadCount++;
            if (Store.TryGetValue(id, out var document))
            {
                return Task.FromResult(new DocumentPersistenceResult(
                    DocumentPersistenceStatus.Succeeded, document, null));
            }
            return Task.FromResult(new DocumentPersistenceResult(
                DocumentPersistenceStatus.NotFound, null, null));
        }

        public Task<DocumentPersistenceResult> SaveAsync(
            Document document,
            long? expectedVersion,
            CancellationToken cancellationToken = default)
        {
            SaveCount++;
            if (Store.TryGetValue(document.Id, out var existing))
            {
                var currentVersion = existing.Version;
                if (expectedVersion is null)
                {
                    return Task.FromResult(new DocumentPersistenceResult(
                        DocumentPersistenceStatus.ConcurrencyConflict,
                        null,
                        "Expected null but document exists."));
                }
                if (expectedVersion.Value != currentVersion)
                {
                    return Task.FromResult(new DocumentPersistenceResult(
                        DocumentPersistenceStatus.ConcurrencyConflict,
                        null,
                        "Version mismatch."));
                }
            }
            else
            {
                if (expectedVersion is not null)
                {
                    return Task.FromResult(new DocumentPersistenceResult(
                        DocumentPersistenceStatus.ConcurrencyConflict,
                        null,
                        "Expected version supplied but document absent."));
                }
            }
            var next = document with { Version = (existing?.Version ?? 0) + 1 };
            Store[document.Id] = next;
            return Task.FromResult(new DocumentPersistenceResult(
                DocumentPersistenceStatus.Succeeded, next, null));
        }

        public Task<DocumentPersistenceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            DeleteCount++;
            if (Store.Remove(id))
            {
                return Task.FromResult(new DocumentPersistenceResult(
                    DocumentPersistenceStatus.Succeeded, null, null));
            }
            return Task.FromResult(new DocumentPersistenceResult(
                DocumentPersistenceStatus.NotFound, null, null));
        }
    }

    [Fact]
    public async Task StoreLoadAsyncReturnsNotFoundForUnknownAggregate()
    {
        var repository = new InMemoryDocumentRepository();
        var store = new AggregateStore(repository);

        var result = await store.LoadAsync(Guid.NewGuid());

        Assert.Equal(DocumentPersistenceStatus.NotFound, result.Status);
        Assert.Null(result.Document);
    }

    [Fact]
    public async Task StoreSaveAsyncCreatesNewDocumentAtVersionOne()
    {
        var repository = new InMemoryDocumentRepository();
        var store = new AggregateStore(repository);
        var aggregate = SampleAggregate.CreateNew("alpha");
        var document = SerializeSample(aggregate);

        var result = await store.SaveAsync(document, expectedVersion: null);

        Assert.Equal(DocumentPersistenceStatus.Succeeded, result.Status);
        Assert.NotNull(result.Document);
        Assert.Equal(1, result.Document!.Version);
    }

    [Fact]
    public async Task StoreSaveAsyncRejectsUpdateWithNullExpectedVersion()
    {
        var repository = new InMemoryDocumentRepository();
        var store = new AggregateStore(repository);
        var aggregate = SampleAggregate.CreateNew("alpha");
        var document = SerializeSample(aggregate);
        await store.SaveAsync(document, expectedVersion: null);

        var update = document with { Version = 1 };
        var second = await store.SaveAsync(update, expectedVersion: null);

        Assert.Equal(DocumentPersistenceStatus.ConcurrencyConflict, second.Status);
    }

    [Fact]
    public async Task StoreSaveAsyncRejectsUpdateWithWrongVersion()
    {
        var repository = new InMemoryDocumentRepository();
        var store = new AggregateStore(repository);
        var aggregate = SampleAggregate.CreateNew("alpha");
        var document = SerializeSample(aggregate);
        await store.SaveAsync(document, expectedVersion: null);

        var second = await store.SaveAsync(document, expectedVersion: 999);

        Assert.Equal(DocumentPersistenceStatus.ConcurrencyConflict, second.Status);
    }

    [Fact]
    public async Task StoreDeleteAsyncRemovesDocument()
    {
        var repository = new InMemoryDocumentRepository();
        var store = new AggregateStore(repository);
        var aggregate = SampleAggregate.CreateNew("alpha");
        var document = SerializeSample(aggregate);
        await store.SaveAsync(document, expectedVersion: null);

        var delete = await store.DeleteAsync(aggregate.Id);
        Assert.Equal(DocumentPersistenceStatus.Succeeded, delete.Status);

        var load = await store.LoadAsync(aggregate.Id);
        Assert.Equal(DocumentPersistenceStatus.NotFound, load.Status);
    }

    [Fact]
    public async Task AggregateLifecycleLoadMutateSaveRoundTrip()
    {
        var repository = new InMemoryDocumentRepository();
        var store = new AggregateStore(repository);
        var newAggregate = SampleAggregate.CreateNew("alpha");
        await store.SaveAsync(SerializeSample(newAggregate), expectedVersion: null);

        var loaded = await store.LoadAsync(newAggregate.Id);
        Assert.Equal(DocumentPersistenceStatus.Succeeded, loaded.Status);
        var (rehydrated, version) = DeserializeSample(loaded.Document!);
        Assert.Equal("alpha", rehydrated.Name);
        Assert.Equal(1, version);

        rehydrated.Rename("beta");
        var renameDocument = SerializeSample(rehydrated) with { Version = version };
        var saved = await store.SaveAsync(renameDocument, expectedVersion: version);
        Assert.Equal(DocumentPersistenceStatus.Succeeded, saved.Status);
        Assert.Equal(2, saved.Document!.Version);

        var reloaded = await store.LoadAsync(newAggregate.Id);
        var (final, finalVersion) = DeserializeSample(reloaded.Document!);
        Assert.Equal("beta", final.Name);
        Assert.Equal(2, finalVersion);
    }

    [Fact]
    public async Task OrchestrationDoesNotPublishDomainEvents()
    {
        var repository = new InMemoryDocumentRepository();
        var store = new AggregateStore(repository);
        var aggregate = SampleAggregate.CreateNew("alpha");

        await store.SaveAsync(SerializeSample(aggregate), expectedVersion: null);
        aggregate.Rename("beta");
        await store.SaveAsync(SerializeSample(aggregate) with { Version = 1 }, expectedVersion: 1);

        Assert.Equal(2, aggregate.UncommittedEvents.Count);
    }

    [Fact]
    public async Task PersistenceFailureIsNotReportedAsSuccess()
    {
        var repository = new InMemoryDocumentRepository();
        var store = new AggregateStore(repository);

        var result = await store.SaveAsync(
            new Document(Guid.NewGuid(), "x", "{}", Version: 0),
            expectedVersion: 0);

        Assert.Equal(DocumentPersistenceStatus.ConcurrencyConflict, result.Status);
        Assert.False(repository.Store.ContainsKey(result.Document?.Id ?? Guid.Empty));
    }
}
