using Chronicle.RuleSets.Abstractions.PackageSources;

namespace Chronicle.RuleSets.Abstractions.Runtime;

public sealed record RuleSetRuntimeIdentity(
    string PackageId,
    string PackageVersion,
    string RuntimeName,
    int SupportedRuleSetContractVersion);

public sealed record RuleSetRuntimeMetadata(
    RuleSetRuntimeIdentity Identity,
    string DeclaredScopeId,
    IReadOnlyList<RuleSetOperationDescriptor> Operations);

public sealed record RuleSetOperationDescriptor(
    string OperationKey,
    string CapabilityKey,
    RuleSetOperationStatus Status);

public enum RuleSetOperationStatus
{
    Declared,
    Enabled,
    Disabled
}

public sealed record RuleSetOperationRequest(
    string PackageId,
    string PackageVersion,
    string OperationKey,
    IReadOnlyDictionary<string, string> Inputs);

public sealed record RuleSetOperationResult(
    bool Succeeded,
    RuleSetOperationFailureCode? FailureCode,
    IReadOnlyList<RuleSetRuntimeFinding> Findings,
    IReadOnlyDictionary<string, string> Outputs);

public sealed record RuleSetRuntimeFinding(
    RuleSetRuntimeFindingSeverity Severity,
    string Code,
    string Message);

public enum RuleSetRuntimeFindingSeverity
{
    Information,
    Warning,
    Error
}

public enum RuleSetOperationFailureCode
{
    RuntimeNotRegistered,
    OperationUndeclared,
    OperationDisabled,
    OperationNotImplemented,
    InvalidRequest
}

public interface IRuleSetRuntime
{
    RuleSetRuntimeMetadata Metadata { get; }

    RuleSetOperationResult Execute(RuleSetOperationRequest request);
}

public sealed record RuleSetRuntimeRegistrationRequest(
    RuleSetPackageCatalog Catalog,
    IReadOnlyList<IRuleSetRuntime> Runtimes);

public sealed record RuleSetRuntimeRegistrationResult(
    IReadOnlyList<RegisteredRuleSetRuntimeDescriptor> RegisteredRuntimes,
    IReadOnlyList<RuleSetRuntimeRegistrationRejection> RejectedRuntimes,
    RuleSetRuntimeRegistry Registry);

public sealed record RegisteredRuleSetRuntimeDescriptor(
    RuleSetRuntimeIdentity Identity,
    string DeclaredScopeId,
    IReadOnlyList<RuleSetOperationDescriptor> Operations);

public sealed record RuleSetRuntimeRegistrationRejection(
    RuleSetRuntimeIdentity? Identity,
    RuleSetRuntimeRegistrationErrorCode Code,
    string Message);

public enum RuleSetRuntimeRegistrationErrorCode
{
    MissingRegisteredPackage,
    PackageIdentityMismatch,
    DuplicateRuntime,
    IncompatibleRuntime,
    UndeclaredOperation,
    DisabledOperationMismatch
}

public sealed class RuleSetRuntimeRegistry
{
    private readonly RuntimeEntry[] entries;

    internal RuleSetRuntimeRegistry(IEnumerable<RuntimeEntry> entries)
    {
        this.entries = entries
            .OrderBy(entry => entry.Runtime.Metadata.Identity.PackageId, StringComparer.Ordinal)
            .ThenBy(entry => entry.Runtime.Metadata.Identity.PackageVersion, StringComparer.Ordinal)
            .ToArray();
    }

    public IReadOnlyList<RegisteredRuleSetRuntimeDescriptor> RegisteredRuntimes => entries
        .Select(entry => RuleSetRuntimeRegistrationService.ToDescriptor(entry.Runtime))
        .ToArray();

    public RegisteredRuleSetRuntimeDescriptor? FindRuntime(string packageId, string packageVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(packageVersion);

        var runtime = entries.SingleOrDefault(entry =>
            StringComparer.Ordinal.Equals(entry.Runtime.Metadata.Identity.PackageId, packageId) &&
            StringComparer.Ordinal.Equals(entry.Runtime.Metadata.Identity.PackageVersion, packageVersion));

        return runtime is null ? null : RuleSetRuntimeRegistrationService.ToDescriptor(runtime.Runtime);
    }

    public RuleSetOperationDescriptor? FindOperation(string packageId, string packageVersion, string operationKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationKey);

        return FindRuntime(packageId, packageVersion)?.Operations
            .SingleOrDefault(operation => StringComparer.Ordinal.Equals(operation.OperationKey, operationKey));
    }

    public RuleSetOperationResult Execute(RuleSetOperationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entry = entries.SingleOrDefault(candidate =>
            StringComparer.Ordinal.Equals(candidate.Runtime.Metadata.Identity.PackageId, request.PackageId) &&
            StringComparer.Ordinal.Equals(candidate.Runtime.Metadata.Identity.PackageVersion, request.PackageVersion));

        if (entry is null)
        {
            return Failure(RuleSetOperationFailureCode.RuntimeNotRegistered, "Runtime is not registered.");
        }

        var operation = entry.Runtime.Metadata.Operations.SingleOrDefault(candidate =>
            StringComparer.Ordinal.Equals(candidate.OperationKey, request.OperationKey));

        if (operation is null)
        {
            return Failure(RuleSetOperationFailureCode.OperationUndeclared, "Operation is not declared by the runtime.");
        }

        if (operation.Status == RuleSetOperationStatus.Disabled)
        {
            return Failure(RuleSetOperationFailureCode.OperationDisabled, "Operation is disabled by the registered package scope.");
        }

        return entry.Runtime.Execute(request);
    }

    private static RuleSetOperationResult Failure(RuleSetOperationFailureCode code, string message)
    {
        return new RuleSetOperationResult(
            false,
            code,
            [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, code.ToString(), message)],
            new Dictionary<string, string>(StringComparer.Ordinal));
    }
}

public static class RuleSetRuntimeRegistrationService
{
    public static RuleSetRuntimeRegistrationResult Register(RuleSetRuntimeRegistrationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var accepted = new List<RuntimeEntry>();
        var rejected = new List<RuleSetRuntimeRegistrationRejection>();

        foreach (var runtime in request.Runtimes
                     .OrderBy(runtime => runtime.Metadata.Identity.PackageId, StringComparer.Ordinal)
                     .ThenBy(runtime => runtime.Metadata.Identity.PackageVersion, StringComparer.Ordinal)
                     .ThenBy(runtime => runtime.Metadata.Identity.RuntimeName, StringComparer.Ordinal))
        {
            var package = request.Catalog.FindByPackageIdAndVersion(
                runtime.Metadata.Identity.PackageId,
                runtime.Metadata.Identity.PackageVersion);

            var rejection = ValidateRuntime(runtime, package);
            if (rejection is not null)
            {
                rejected.Add(rejection);
                continue;
            }

            accepted.Add(new RuntimeEntry(runtime, package!));
        }

        RejectDuplicates(accepted, rejected);

        var duplicateIdentities = rejected
            .Where(rejection => rejection.Code == RuleSetRuntimeRegistrationErrorCode.DuplicateRuntime && rejection.Identity is not null)
            .Select(rejection => $"{rejection.Identity!.PackageId}@{rejection.Identity.PackageVersion}")
            .ToHashSet(StringComparer.Ordinal);

        var registered = accepted
            .Where(entry => !duplicateIdentities.Contains($"{entry.Runtime.Metadata.Identity.PackageId}@{entry.Runtime.Metadata.Identity.PackageVersion}"))
            .OrderBy(entry => entry.Runtime.Metadata.Identity.PackageId, StringComparer.Ordinal)
            .ThenBy(entry => entry.Runtime.Metadata.Identity.PackageVersion, StringComparer.Ordinal)
            .ToArray();

        return new RuleSetRuntimeRegistrationResult(
            registered.Select(entry => ToDescriptor(entry.Runtime)).ToArray(),
            rejected.OrderBy(rejection => rejection.Identity?.PackageId ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(rejection => rejection.Identity?.PackageVersion ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(rejection => rejection.Code.ToString(), StringComparer.Ordinal)
                .ToArray(),
            new RuleSetRuntimeRegistry(registered));
    }

    internal static RegisteredRuleSetRuntimeDescriptor ToDescriptor(IRuleSetRuntime runtime)
    {
        return new RegisteredRuleSetRuntimeDescriptor(
            runtime.Metadata.Identity,
            runtime.Metadata.DeclaredScopeId,
            runtime.Metadata.Operations
                .OrderBy(operation => operation.OperationKey, StringComparer.Ordinal)
                .ToArray());
    }

    private static RuleSetRuntimeRegistrationRejection? ValidateRuntime(
        IRuleSetRuntime runtime,
        RegisteredRuleSetPackageDescriptor? package)
    {
        var identity = runtime.Metadata.Identity;

        if (package is null)
        {
            return new RuleSetRuntimeRegistrationRejection(identity, RuleSetRuntimeRegistrationErrorCode.MissingRegisteredPackage, "Runtime has no matching registered package.");
        }

        if (!StringComparer.Ordinal.Equals(runtime.Metadata.DeclaredScopeId, package.DeclaredScopeId))
        {
            return new RuleSetRuntimeRegistrationRejection(identity, RuleSetRuntimeRegistrationErrorCode.PackageIdentityMismatch, "Runtime declared scope does not match the registered package.");
        }

        if (identity.SupportedRuleSetContractVersion != package.RuleSetContractVersion)
        {
            return new RuleSetRuntimeRegistrationRejection(identity, RuleSetRuntimeRegistrationErrorCode.IncompatibleRuntime, "Runtime contract version is incompatible with the registered package.");
        }

        foreach (var operation in runtime.Metadata.Operations)
        {
            var declaredCapability = package.Capabilities.Contains(operation.CapabilityKey, StringComparer.Ordinal);
            var declaredDisabledOperation = package.DisabledOperations.Contains(operation.OperationKey, StringComparer.Ordinal);

            if (!declaredCapability && !declaredDisabledOperation)
            {
                return new RuleSetRuntimeRegistrationRejection(identity, RuleSetRuntimeRegistrationErrorCode.UndeclaredOperation, $"Operation '{operation.OperationKey}' is not declared by package scope.");
            }

            if (declaredDisabledOperation && operation.Status != RuleSetOperationStatus.Disabled)
            {
                return new RuleSetRuntimeRegistrationRejection(identity, RuleSetRuntimeRegistrationErrorCode.DisabledOperationMismatch, $"Operation '{operation.OperationKey}' must remain disabled.");
            }
        }

        return null;
    }

    private static void RejectDuplicates(
        IReadOnlyCollection<RuntimeEntry> entries,
        List<RuleSetRuntimeRegistrationRejection> rejected)
    {
        foreach (var duplicateGroup in entries
                     .GroupBy(entry => $"{entry.Runtime.Metadata.Identity.PackageId}@{entry.Runtime.Metadata.Identity.PackageVersion}", StringComparer.Ordinal)
                     .Where(group => group.Count() > 1)
                     .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            foreach (var entry in duplicateGroup)
            {
                rejected.Add(new RuleSetRuntimeRegistrationRejection(
                    entry.Runtime.Metadata.Identity,
                    RuleSetRuntimeRegistrationErrorCode.DuplicateRuntime,
                    "Runtime identity/version is already registered."));
            }
        }
    }
}

internal sealed record RuntimeEntry(IRuleSetRuntime Runtime, RegisteredRuleSetPackageDescriptor Package);
