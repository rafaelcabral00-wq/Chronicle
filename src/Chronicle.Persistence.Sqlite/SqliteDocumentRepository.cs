using Chronicle.Application.Persistence;
using Microsoft.Data.Sqlite;

namespace Chronicle.Persistence.Sqlite;

/// <summary>
/// SQLite-backed implementation of <see cref="IDocumentRepository"/>.
/// <para>
/// E1 persists every document into a single table (<c>documents</c>) with
/// columns: <c>Id</c> (16-byte binary), <c>ContentType</c> (TEXT),
/// <c>PayloadJson</c> (TEXT), <c>Version</c> (INTEGER). The schema is
/// created on first connection open and is generic — no Pack/Totem or
/// other aggregate-specific columns are introduced.
/// </para>
/// <para>
/// Connection lifetime is owned by the caller: a new connection is
/// opened per operation, used inside a single transaction, and disposed
/// deterministically. There is no global singleton database, no
/// environment-specific path, and no leaked handle.
/// </para>
/// <para>
/// E1 deliberately does not implement migrations, schema versioning,
/// multi-tenant partitioning, or connection pooling. They will be
/// introduced only when a concrete requirement demonstrates a need.
/// </para>
/// </summary>
public sealed class SqliteDocumentRepository : IDocumentRepository
{
    private readonly string connectionString;

    public SqliteDocumentRepository(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string must not be empty.", nameof(connectionString));
        }
        this.connectionString = connectionString;
    }

    public async Task<DocumentPersistenceResult> LoadAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await EnsureSchemaAsync(connection, cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT ContentType, PayloadJson, Version FROM documents WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id.ToByteArray());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return new DocumentPersistenceResult(DocumentPersistenceStatus.NotFound, null, null);
        }

        var contentType = reader.GetString(0);
        var payloadJson = reader.GetString(1);
        var version = reader.GetInt64(2);
        return new DocumentPersistenceResult(
            DocumentPersistenceStatus.Succeeded,
            new Document(id, contentType, payloadJson, version),
            null);
    }

    public async Task<DocumentPersistenceResult> SaveAsync(
        Document document,
        long? expectedVersion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await EnsureSchemaAsync(connection, cancellationToken).ConfigureAwait(false);

        await using var transaction = (SqliteTransaction)await connection
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        long currentVersion;
        bool exists;
        await using (var readCommand = connection.CreateCommand())
        {
            readCommand.Transaction = transaction;
            readCommand.CommandText = "SELECT Version FROM documents WHERE Id = $id";
            readCommand.Parameters.AddWithValue("$id", document.Id.ToByteArray());
            var result = await readCommand
                .ExecuteScalarAsync(cancellationToken)
                .ConfigureAwait(false);
            if (result is null || result is DBNull)
            {
                exists = false;
                currentVersion = 0;
            }
            else
            {
                exists = true;
                currentVersion = (long)result;
            }
        }

        if (exists)
        {
            if (expectedVersion is null)
            {
                return new DocumentPersistenceResult(
                    DocumentPersistenceStatus.ConcurrencyConflict,
                    null,
                    "Expected version was null but the document already exists.");
            }

            if (expectedVersion.Value != currentVersion)
            {
                return new DocumentPersistenceResult(
                    DocumentPersistenceStatus.ConcurrencyConflict,
                    null,
                    $"Expected version {expectedVersion.Value} did not match stored version {currentVersion}.");
            }
        }
        else
        {
            if (expectedVersion is not null)
            {
                return new DocumentPersistenceResult(
                    DocumentPersistenceStatus.ConcurrencyConflict,
                    null,
                    $"Expected version {expectedVersion.Value} was supplied but the document does not exist.");
            }
        }

        var nextVersion = currentVersion + 1;
        await using (var writeCommand = connection.CreateCommand())
        {
            writeCommand.Transaction = transaction;
            writeCommand.CommandText = exists
                ? @"UPDATE documents SET ContentType = $ct, PayloadJson = $p, Version = $v WHERE Id = $id"
                : @"INSERT INTO documents (Id, ContentType, PayloadJson, Version) VALUES ($id, $ct, $p, $v)";
            writeCommand.Parameters.AddWithValue("$id", document.Id.ToByteArray());
            writeCommand.Parameters.AddWithValue("$ct", document.ContentType);
            writeCommand.Parameters.AddWithValue("$p", document.PayloadJson);
            writeCommand.Parameters.AddWithValue("$v", nextVersion);
            var rowsAffected = await writeCommand
                .ExecuteNonQueryAsync(cancellationToken)
                .ConfigureAwait(false);
            if (rowsAffected != 1)
            {
                return new DocumentPersistenceResult(
                    DocumentPersistenceStatus.PersistenceFailure,
                    null,
                    $"Unexpected rows-affected count: {rowsAffected}.");
            }
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new DocumentPersistenceResult(
            DocumentPersistenceStatus.Succeeded,
            document with { Version = nextVersion },
            null);
    }

    public async Task<DocumentPersistenceResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await EnsureSchemaAsync(connection, cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM documents WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id.ToByteArray());

        var rowsAffected = await command
            .ExecuteNonQueryAsync(cancellationToken)
            .ConfigureAwait(false);
        if (rowsAffected == 0)
        {
            return new DocumentPersistenceResult(DocumentPersistenceStatus.NotFound, null, null);
        }

        return new DocumentPersistenceResult(DocumentPersistenceStatus.Succeeded, null, null);
    }

    private static async Task EnsureSchemaAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS documents (
                Id BLOB NOT NULL PRIMARY KEY,
                ContentType TEXT NOT NULL,
                PayloadJson TEXT NOT NULL,
                Version INTEGER NOT NULL
            )";
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
