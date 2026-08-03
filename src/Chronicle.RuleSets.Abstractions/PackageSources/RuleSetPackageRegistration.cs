namespace Chronicle.RuleSets.Abstractions.PackageSources;

public sealed record RuleSetPackageRegistrationRequest(
    IReadOnlyList<RuleSetPackageSourceDescriptor> PackageSources,
    int SupportedRuleSetContractVersion);

public sealed record RuleSetPackageRegistrationResult(
    RuleSetPackageRegistrationStatus Status,
    IReadOnlyList<RegisteredRuleSetPackageDescriptor> AvailablePackages,
    IReadOnlyList<RegisteredRuleSetPackageDescriptor> CompatiblePackages,
    IReadOnlyList<RegisteredRuleSetPackageDescriptor> IncompatiblePackages,
    IReadOnlyList<RuleSetPackageRegistrationRejection> RejectedPackages,
    RuleSetPackageCatalog Catalog);

public sealed record RegisteredRuleSetPackageDescriptor(
    string PackageId,
    string PackageIdStatus,
    string PackageVersion,
    string DeclaredScopeId,
    string MinimumChronicleVersion,
    string MaximumChronicleVersionPolicy,
    int RuleSetContractVersion,
    IReadOnlyList<string> SupportedLocales,
    IReadOnlyList<string> Capabilities,
    IReadOnlyList<string> DisabledOperations,
    string PackageSourcePath);

public sealed record RuleSetPackageRegistrationRejection(
    string PackageSourcePath,
    string PackageId,
    string PackageVersion,
    RuleSetPackageRegistrationErrorCode Code,
    string Message);

public enum RuleSetPackageRegistrationStatus
{
    Available,
    Rejected
}

public enum RuleSetPackageRegistrationErrorCode
{
    MissingValidationEvidence,
    DescriptorRejectedOrInvalid,
    ValidationFindingsPresent,
    DuplicatePackageIdentity
}

public sealed class RuleSetPackageCatalog
{
    private readonly RegisteredRuleSetPackageDescriptor[] packages;

    public RuleSetPackageCatalog(IEnumerable<RegisteredRuleSetPackageDescriptor> packages)
    {
        this.packages = packages
            .OrderBy(package => package.PackageId, StringComparer.Ordinal)
            .ThenBy(package => package.PackageVersion, StringComparer.Ordinal)
            .ThenBy(package => package.PackageSourcePath, StringComparer.Ordinal)
            .ToArray();
    }

    public IReadOnlyList<RegisteredRuleSetPackageDescriptor> Packages => packages.ToArray();

    public IReadOnlyList<RegisteredRuleSetPackageDescriptor> FindByPackageId(string packageId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        return packages
            .Where(package => StringComparer.Ordinal.Equals(package.PackageId, packageId))
            .OrderBy(package => package.PackageVersion, StringComparer.Ordinal)
            .ThenBy(package => package.PackageSourcePath, StringComparer.Ordinal)
            .ToArray();
    }

    public RegisteredRuleSetPackageDescriptor? FindByPackageIdAndVersion(string packageId, string packageVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(packageVersion);

        return packages.SingleOrDefault(package =>
            StringComparer.Ordinal.Equals(package.PackageId, packageId) &&
            StringComparer.Ordinal.Equals(package.PackageVersion, packageVersion));
    }
}

public static class RuleSetPackageRegistrationService
{
    private const string RequiredValidationEvidence = "canonical-package-source-validation-passed:v1";

    public static RuleSetPackageRegistrationResult Register(RuleSetPackageRegistrationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var candidates = new List<RegisteredRuleSetPackageDescriptor>();
        var rejections = new List<RuleSetPackageRegistrationRejection>();

        foreach (var descriptor in request.PackageSources
                     .OrderBy(package => package.PackageId, StringComparer.Ordinal)
                     .ThenBy(package => package.PackageVersion, StringComparer.Ordinal)
                     .ThenBy(package => package.PackageSourcePath, StringComparer.Ordinal))
        {
            var rejection = ValidateDescriptor(descriptor);
            if (rejection is not null)
            {
                rejections.Add(rejection);
                continue;
            }

            candidates.Add(ToRegisteredDescriptor(descriptor));
        }

        RejectDuplicates(candidates, rejections);

        var rejectedIdentities = rejections
            .Where(rejection => rejection.Code == RuleSetPackageRegistrationErrorCode.DuplicatePackageIdentity)
            .Select(rejection => $"{rejection.PackageId}@{rejection.PackageVersion}")
            .ToHashSet(StringComparer.Ordinal);

        var available = candidates
            .Where(candidate => !rejectedIdentities.Contains($"{candidate.PackageId}@{candidate.PackageVersion}"))
            .OrderBy(candidate => candidate.PackageId, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.PackageVersion, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.PackageSourcePath, StringComparer.Ordinal)
            .ToArray();

        var compatible = available
            .Where(package => package.RuleSetContractVersion == request.SupportedRuleSetContractVersion)
            .ToArray();

        var incompatible = available
            .Where(package => package.RuleSetContractVersion != request.SupportedRuleSetContractVersion)
            .ToArray();

        return new RuleSetPackageRegistrationResult(
            rejections.Count == 0 ? RuleSetPackageRegistrationStatus.Available : RuleSetPackageRegistrationStatus.Rejected,
            available,
            compatible,
            incompatible,
            rejections
                .OrderBy(rejection => rejection.PackageId, StringComparer.Ordinal)
                .ThenBy(rejection => rejection.PackageVersion, StringComparer.Ordinal)
                .ThenBy(rejection => rejection.PackageSourcePath, StringComparer.Ordinal)
                .ThenBy(rejection => rejection.Code.ToString(), StringComparer.Ordinal)
                .ToArray(),
            new RuleSetPackageCatalog(available));
    }

    private static RuleSetPackageRegistrationRejection? ValidateDescriptor(RuleSetPackageSourceDescriptor descriptor)
    {
        if (!StringComparer.Ordinal.Equals(descriptor.ValidationEvidence, RequiredValidationEvidence))
        {
            return Rejection(descriptor, RuleSetPackageRegistrationErrorCode.MissingValidationEvidence, "Package descriptor lacks required canonical validation evidence.");
        }

        if (descriptor.ValidationStatus != RuleSetPackageSourceDiscoveryValidationStatus.Valid)
        {
            return Rejection(descriptor, RuleSetPackageRegistrationErrorCode.DescriptorRejectedOrInvalid, "Only valid discovered package source descriptors may be registered.");
        }

        if (descriptor.Findings.Count > 0)
        {
            return Rejection(descriptor, RuleSetPackageRegistrationErrorCode.ValidationFindingsPresent, "Package descriptor contains validation findings.");
        }

        return null;
    }

    private static void RejectDuplicates(
        IReadOnlyCollection<RegisteredRuleSetPackageDescriptor> candidates,
        List<RuleSetPackageRegistrationRejection> rejections)
    {
        foreach (var duplicateGroup in candidates
                     .GroupBy(package => $"{package.PackageId}@{package.PackageVersion}", StringComparer.Ordinal)
                     .Where(group => group.Count() > 1)
                     .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            foreach (var package in duplicateGroup.OrderBy(package => package.PackageSourcePath, StringComparer.Ordinal))
            {
                rejections.Add(new RuleSetPackageRegistrationRejection(
                    package.PackageSourcePath,
                    package.PackageId,
                    package.PackageVersion,
                    RuleSetPackageRegistrationErrorCode.DuplicatePackageIdentity,
                    $"Package identity '{package.PackageId}' version '{package.PackageVersion}' is already registered."));
            }
        }
    }

    private static RegisteredRuleSetPackageDescriptor ToRegisteredDescriptor(RuleSetPackageSourceDescriptor descriptor)
    {
        return new RegisteredRuleSetPackageDescriptor(
            descriptor.PackageId,
            descriptor.PackageIdStatus,
            descriptor.PackageVersion,
            descriptor.DeclaredScopeId,
            descriptor.MinimumChronicleVersion,
            descriptor.MaximumChronicleVersionPolicy,
            descriptor.RuleSetContractVersion,
            descriptor.SupportedLocales.Order(StringComparer.Ordinal).ToArray(),
            descriptor.Capabilities.Order(StringComparer.Ordinal).ToArray(),
            descriptor.DisabledOperations.Order(StringComparer.Ordinal).ToArray(),
            descriptor.PackageSourcePath);
    }

    private static RuleSetPackageRegistrationRejection Rejection(
        RuleSetPackageSourceDescriptor descriptor,
        RuleSetPackageRegistrationErrorCode code,
        string message)
    {
        return new RuleSetPackageRegistrationRejection(
            descriptor.PackageSourcePath,
            descriptor.PackageId,
            descriptor.PackageVersion,
            code,
            message);
    }
}
