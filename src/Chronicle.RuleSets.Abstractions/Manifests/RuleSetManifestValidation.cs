using System.Text.Json;
using System.Text.RegularExpressions;

namespace Chronicle.RuleSets.Abstractions.Manifests;

public sealed record RuleSetManifestValidationResult(
    bool IsValid,
    IReadOnlyList<RuleSetManifestValidationError> Errors)
{
    public static RuleSetManifestValidationResult Valid { get; } = new(true, []);

    public static RuleSetManifestValidationResult Invalid(IEnumerable<RuleSetManifestValidationError> errors)
    {
        var materialized = errors.OrderBy(error => error.Path, StringComparer.Ordinal)
            .ThenBy(error => error.Code.ToString(), StringComparer.Ordinal)
            .ThenBy(error => error.Message, StringComparer.Ordinal)
            .ToArray();

        return new RuleSetManifestValidationResult(false, materialized);
    }
}

public sealed record RuleSetManifestValidationError(
    RuleSetManifestValidationErrorCode Code,
    string Path,
    string Message);

public enum RuleSetManifestValidationErrorCode
{
    MissingRequiredField,
    MalformedSemanticVersion,
    UnsupportedContractVersion,
    DuplicateCapability,
    ContradictoryCapabilityDeclaration,
    InvalidLocalizationDeclaration,
    InvalidCompatibilityRange,
    NonDeterministicOrdering,
    InvalidPackageIdStatus,
    InvalidDisabledOperation
}

public static partial class RuleSetManifestValidator
{
    private const int SupportedContractVersion = 1;

    public static RuleSetManifestValidationResult ValidateJson(string manifestJson)
    {
        ArgumentNullException.ThrowIfNull(manifestJson);

        using var document = JsonDocument.Parse(manifestJson);
        return Validate(document.RootElement);
    }

    public static RuleSetManifestValidationResult Validate(JsonElement manifest)
    {
        var errors = new List<RuleSetManifestValidationError>();

        foreach (var field in RuleSetManifestSchemaVersions.Version1.RequiredFields)
        {
            if (!manifest.TryGetProperty(field, out _))
            {
                errors.Add(new RuleSetManifestValidationError(
                    RuleSetManifestValidationErrorCode.MissingRequiredField,
                    field,
                    $"Required manifest field '{field}' is missing."));
            }
        }

        if (errors.Count > 0)
        {
            return RuleSetManifestValidationResult.Invalid(errors);
        }

        ValidateContractVersion(manifest, errors);
        ValidatePackageIdentity(manifest, errors);
        ValidateSemanticVersion(manifest.GetProperty("packageVersion").GetString(), "packageVersion", errors);
        ValidateLocales(manifest, errors);
        ValidateCompatibility(manifest, errors);
        ValidateCapabilities(manifest, errors);
        ValidateDisabledOperations(manifest, errors);
        ValidateOrderedStringArrayProperty(manifest, "supportedLocales", errors);

        if (manifest.TryGetProperty("excludedMechanics", out var excludedMechanics))
        {
            ValidateOrderedArray(excludedMechanics, "excludedMechanics", errors);
        }

        return errors.Count == 0
            ? RuleSetManifestValidationResult.Valid
            : RuleSetManifestValidationResult.Invalid(errors);
    }

    private static void ValidateContractVersion(JsonElement manifest, List<RuleSetManifestValidationError> errors)
    {
        if (manifest.GetProperty("contractVersion").GetInt32() != SupportedContractVersion)
        {
            errors.Add(new RuleSetManifestValidationError(
                RuleSetManifestValidationErrorCode.UnsupportedContractVersion,
                "contractVersion",
                "The manifest contract version is not supported by this validator."));
        }
    }

    private static void ValidatePackageIdentity(JsonElement manifest, List<RuleSetManifestValidationError> errors)
    {
        var status = manifest.GetProperty("packageIdStatus").GetString();
        if (!StringComparer.Ordinal.Equals(status, "final") &&
            !StringComparer.Ordinal.Equals(status, "provisional-governance-pending"))
        {
            errors.Add(new RuleSetManifestValidationError(
                RuleSetManifestValidationErrorCode.InvalidPackageIdStatus,
                "packageIdStatus",
                "PackageId status must be final or provisional-governance-pending."));
        }
    }

    private static void ValidateLocales(JsonElement manifest, List<RuleSetManifestValidationError> errors)
    {
        var canonicalLanguage = manifest.GetProperty("canonicalLanguage").GetString();
        var supportedLocales = manifest.GetProperty("supportedLocales")
            .EnumerateArray()
            .Select(locale => locale.GetString())
            .ToArray();

        if (string.IsNullOrWhiteSpace(canonicalLanguage) ||
            !LocalePattern().IsMatch(canonicalLanguage) ||
            supportedLocales.Any(locale => string.IsNullOrWhiteSpace(locale) || !LocalePattern().IsMatch(locale!)) ||
            !supportedLocales.Contains(canonicalLanguage, StringComparer.Ordinal))
        {
            errors.Add(new RuleSetManifestValidationError(
                RuleSetManifestValidationErrorCode.InvalidLocalizationDeclaration,
                "supportedLocales",
                "Supported locales must be stable locale tags and include the canonical language."));
        }
    }

    private static void ValidateCompatibility(JsonElement manifest, List<RuleSetManifestValidationError> errors)
    {
        var compatibility = manifest.GetProperty("compatibility");
        var minimum = compatibility.GetProperty("minimumChronicleVersion").GetString();
        var ruleSetContractVersion = compatibility.GetProperty("ruleSetContractVersion").GetInt32();
        var policy = compatibility.GetProperty("maximumChronicleVersionPolicy").GetString();

        ValidateSemanticVersion(minimum, "compatibility.minimumChronicleVersion", errors);

        if (ruleSetContractVersion != SupportedContractVersion ||
            !StringComparer.Ordinal.Equals(policy, "same-major-contract"))
        {
            errors.Add(new RuleSetManifestValidationError(
                RuleSetManifestValidationErrorCode.InvalidCompatibilityRange,
                "compatibility",
                "Compatibility must target contract version 1 and use same-major-contract maximum policy."));
        }
    }

    private static void ValidateCapabilities(JsonElement manifest, List<RuleSetManifestValidationError> errors)
    {
        var capabilities = manifest.GetProperty("capabilities")
            .EnumerateArray()
            .Select(capability => capability.GetProperty("capabilityKey").GetString() ?? string.Empty)
            .ToArray();

        var duplicateCapabilities = capabilities.GroupBy(capability => capability, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        foreach (var duplicate in duplicateCapabilities)
        {
            errors.Add(new RuleSetManifestValidationError(
                RuleSetManifestValidationErrorCode.DuplicateCapability,
                "capabilities",
                $"Capability '{duplicate}' is declared more than once."));
        }

        ValidateOrderedValues(capabilities, "capabilities", errors);

        var disabledOperations = manifest.GetProperty("disabledOperations")
            .EnumerateArray()
            .Select(operation => operation.GetProperty("capabilityKey").GetString())
            .Where(capability => !string.IsNullOrWhiteSpace(capability))
            .ToArray();

        var contradictions = capabilities.Intersect(disabledOperations!, StringComparer.Ordinal);
        foreach (var contradiction in contradictions)
        {
            errors.Add(new RuleSetManifestValidationError(
                RuleSetManifestValidationErrorCode.ContradictoryCapabilityDeclaration,
                "disabledOperations",
                $"Capability '{contradiction}' is both supported and disabled."));
        }
    }

    private static void ValidateDisabledOperations(JsonElement manifest, List<RuleSetManifestValidationError> errors)
    {
        var disabledOperationKeys = new List<string>();

        foreach (var operation in manifest.GetProperty("disabledOperations").EnumerateArray())
        {
            var key = operation.GetProperty("operationKey").GetString();
            var status = operation.GetProperty("status").GetString();
            var reasonKey = operation.GetProperty("reasonKey").GetString();
            disabledOperationKeys.Add(key ?? string.Empty);

            if (string.IsNullOrWhiteSpace(key) ||
                !StringComparer.Ordinal.Equals(status, "disabled") ||
                string.IsNullOrWhiteSpace(reasonKey))
            {
                errors.Add(new RuleSetManifestValidationError(
                    RuleSetManifestValidationErrorCode.InvalidDisabledOperation,
                    "disabledOperations",
                    "Disabled operations require operationKey, disabled status, and reasonKey."));
            }
        }

        ValidateOrderedValues(disabledOperationKeys, "disabledOperations", errors);
    }

    private static void ValidateSemanticVersion(string? version, string path, List<RuleSetManifestValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(version) || !SemanticVersionPattern().IsMatch(version))
        {
            errors.Add(new RuleSetManifestValidationError(
                RuleSetManifestValidationErrorCode.MalformedSemanticVersion,
                path,
                $"Value at '{path}' must be a semantic version."));
        }
    }

    private static void ValidateOrderedStringArrayProperty(JsonElement parent, string path, List<RuleSetManifestValidationError> errors)
    {
        var values = parent.GetProperty(path)
            .EnumerateArray()
            .Select(value => value.GetString() ?? string.Empty)
            .ToArray();

        ValidateOrderedValues(values, path, errors);
    }

    private static void ValidateOrderedArray(JsonElement array, string path, List<RuleSetManifestValidationError> errors)
    {
        var values = array.EnumerateArray()
            .Select(value => value.GetString() ?? string.Empty)
            .ToArray();

        ValidateOrderedValues(values, path, errors);
    }

    private static void ValidateOrderedValues(IReadOnlyList<string> values, string path, List<RuleSetManifestValidationError> errors)
    {
        var ordered = values.Order(StringComparer.Ordinal).ToArray();
        if (!values.SequenceEqual(ordered, StringComparer.Ordinal))
        {
            errors.Add(new RuleSetManifestValidationError(
                RuleSetManifestValidationErrorCode.NonDeterministicOrdering,
                path,
                $"Values at '{path}' must be sorted using ordinal ordering."));
        }
    }

    [GeneratedRegex(@"^[a-z]{2}(-[A-Z]{2})?$")]
    private static partial Regex LocalePattern();

    [GeneratedRegex(@"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-[0-9A-Za-z.-]+)?$")]
    private static partial Regex SemanticVersionPattern();
}
