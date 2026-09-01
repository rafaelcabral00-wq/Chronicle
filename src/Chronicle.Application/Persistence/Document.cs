namespace Chronicle.Application.Persistence;

/// <summary>
/// A persisted representation of an aggregate's serialized state.
/// <para>
/// Documents are deliberately provider-neutral and aggregate-agnostic.
/// The Application layer is the only place this type is defined; concrete
/// persistence projects (e.g. <c>Chronicle.Persistence.Sqlite</c>) implement
/// the storage mechanism, and concrete aggregate persistence is mapped by
/// the Application orchestrator at use time.
/// </para>
/// <para>
/// E1 deliberately does not embed aggregate types, content-type registries,
/// or schema metadata. The minimal <see cref="PayloadJson"/> field is a JSON
/// string serialized by the Application orchestrator at the moment of
/// persistence; concrete aggregate state remains in <c>Chronicle.Domain</c>.
/// </para>
/// </summary>
public sealed record Document(
    Guid Id,
    string ContentType,
    string PayloadJson,
    long Version);
