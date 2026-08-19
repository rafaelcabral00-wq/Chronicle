using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfTribeEligibilityRequest(
    string TribeId,
    string? Race,
    IReadOnlyDictionary<string, int?> Backgrounds);

public sealed record WerewolfTribeEligibilityResult(
    bool IsEligible,
    IReadOnlyList<WerewolfTribeEligibilityFinding> Findings);

public sealed record WerewolfTribeEligibilityFinding(
    WerewolfTribeEligibilitySeverity Severity,
    WerewolfTribeEligibilityErrorCode Code,
    string Message);

public enum WerewolfTribeEligibilitySeverity
{
    Information,
    Error
}

public enum WerewolfTribeEligibilityErrorCode
{
    Eligible,
    UnknownTribe,
    RaceBreedIneligible,
    BackgroundMinimumNotMet,
    DependencyUnavailable
}

public static class WerewolfTribeEligibilityService
{
    public static WerewolfTribeEligibilityResult CheckEligibility(WerewolfTribeEligibilityRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.TribeId))
        {
            return new WerewolfTribeEligibilityResult(false, [Error(WerewolfTribeEligibilityErrorCode.UnknownTribe, "Tribe identifier is required.")]);
        }

        var tribe = request.TribeId.Trim();
        if (!StringComparer.Ordinal.Equals(tribe, request.TribeId) || tribe.Any(char.IsWhiteSpace))
        {
            return new WerewolfTribeEligibilityResult(false, [Error(WerewolfTribeEligibilityErrorCode.UnknownTribe, "Tribe identifier must be canonical and whitespace-free.")]);
        }

        if (!WerewolfTribeIdentifiers.Supported.Contains(tribe, StringComparer.Ordinal))
        {
            return new WerewolfTribeEligibilityResult(false, [Error(WerewolfTribeEligibilityErrorCode.UnknownTribe, $"Tribe '{tribe}' is not declared by the current slice.")]);
        }

        var findings = new List<WerewolfTribeEligibilityFinding>();

        var raceFindings = CheckRaceBreedEligibility(tribe, request.Race);
        findings.AddRange(raceFindings);

        var backgroundFindings = CheckBackgroundMinimums(tribe, request.Backgrounds);
        findings.AddRange(backgroundFindings);

        var dependencyFindings = CheckDependencies(tribe, request.Backgrounds);
        findings.AddRange(dependencyFindings);

        var hasErrors = findings.Any(finding => finding.Severity == WerewolfTribeEligibilitySeverity.Error);
        return new WerewolfTribeEligibilityResult(!hasErrors, findings.AsReadOnly());
    }

    public static IReadOnlyList<WerewolfTribeEligibilityFinding> CheckRaceBreedEligibility(string tribeId, string? race)
    {
        if (StringComparer.Ordinal.Equals(tribeId, WerewolfTribeIdentifiers.RedTalons) && !StringComparer.Ordinal.Equals(race, WerewolfRaceIdentifiers.Lupus))
        {
            return [Error(WerewolfTribeEligibilityErrorCode.RaceBreedIneligible, "Red Talons are restricted to Lupus race per source (line 733, 970).")];
        }

        return Array.Empty<WerewolfTribeEligibilityFinding>();
    }

    public static IReadOnlyList<WerewolfTribeEligibilityFinding> CheckBackgroundMinimums(string tribeId, IReadOnlyDictionary<string, int?> backgrounds)
    {
        if (!StringComparer.Ordinal.Equals(tribeId, WerewolfTribeIdentifiers.SilverFangs))
        {
            return Array.Empty<WerewolfTribeEligibilityFinding>();
        }

        if (backgrounds.TryGetValue(WerewolfBackgroundIdentifiers.PureBreed, out var pureBreed) && pureBreed.HasValue)
        {
            if (pureBreed.Value >= 3)
            {
                return Array.Empty<WerewolfTribeEligibilityFinding>();
            }

            return [Error(WerewolfTribeEligibilityErrorCode.BackgroundMinimumNotMet, "Silver Fangs require Pure Breed >= 3.")];
        }

        return [Error(WerewolfTribeEligibilityErrorCode.DependencyUnavailable, "Silver Fangs require Pure Breed >= 3, but Pure Breed is not available in the current character creation slice.")];
    }

    public static IReadOnlyList<WerewolfTribeEligibilityFinding> CheckDependencies(string tribeId, IReadOnlyDictionary<string, int?> backgrounds)
    {
        if (StringComparer.Ordinal.Equals(tribeId, WerewolfTribeIdentifiers.BlackFuries))
        {
            return [Error(WerewolfTribeEligibilityErrorCode.DependencyUnavailable, "Black Furies require female gender, but no gender field is available in the current character creation slice. Deferred to RULESET-COMPLETION-011.")];
        }

        return Array.Empty<WerewolfTribeEligibilityFinding>();
    }

    private static WerewolfTribeEligibilityFinding Error(WerewolfTribeEligibilityErrorCode code, string message)
    {
        return new WerewolfTribeEligibilityFinding(WerewolfTribeEligibilitySeverity.Error, code, message);
    }
}
