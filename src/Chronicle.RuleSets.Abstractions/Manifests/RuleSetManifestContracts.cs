namespace Chronicle.RuleSets.Abstractions.Manifests;

public sealed record RuleSetPackageManifest(
    int ContractVersion,
    string ManifestRole,
    string PackageId,
    string PackageIdStatus,
    string PackageVersion,
    string PublisherId,
    string PackageKind,
    string DisplayNameResourceKey,
    string DescriptionResourceKey,
    string PublicationStatus,
    string PromotionStatus,
    string CanonicalLanguage,
    IReadOnlyList<string> SupportedLocales,
    RuleSetScopeDeclaration DeclaredReleaseScope,
    RuleSetCompatibilityDeclaration Compatibility,
    IReadOnlyList<RuleSetCapabilityDeclaration> Capabilities,
    IReadOnlyList<RuleSetDisabledOperationDeclaration> DisabledOperations,
    IReadOnlyList<string> ExcludedMechanics);

public sealed record RuleSetScopeDeclaration(
    string ScopeId,
    string ScopeStatus,
    string SourceSystemCompleteness,
    string DeclaredScopeCompleteness,
    string ImplementationCompleteness,
    string ValidationCompleteness,
    string PublicationStatus);

public sealed record RuleSetCompatibilityDeclaration(
    string MinimumChronicleVersion,
    string MaximumChronicleVersionPolicy,
    int RuleSetContractVersion);

public sealed record RuleSetCapabilityDeclaration(
    string CapabilityKey,
    string Status);

public sealed record RuleSetDisabledOperationDeclaration(
    string OperationKey,
    string Status,
    string ReasonKey);

public sealed record RuleSetManifestSchema(
    int ContractVersion,
    IReadOnlyList<string> RequiredFields,
    IReadOnlyList<string> DeterministicallyOrderedCollections);

public static class RuleSetManifestSchemaVersions
{
    public static RuleSetManifestSchema Version1 { get; } = new(
        1,
        [
            "contractVersion",
            "manifestRole",
            "packageId",
            "packageIdStatus",
            "packageVersion",
            "publisherId",
            "packageKind",
            "displayNameResourceKey",
            "descriptionResourceKey",
            "publicationStatus",
            "promotionStatus",
            "canonicalLanguage",
            "supportedLocales",
            "declaredReleaseScope",
            "compatibility",
            "capabilities",
            "disabledOperations"
        ],
        [
            "supportedLocales",
            "capabilities.capabilityKey",
            "disabledOperations.operationKey",
            "excludedMechanics"
        ]);
}
