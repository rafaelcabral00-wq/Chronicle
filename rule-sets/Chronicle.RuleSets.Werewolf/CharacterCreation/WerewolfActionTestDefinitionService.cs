namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum WerewolfActionTestDefinitionFindingSeverity
{
    Information,
    Error
}

public sealed record WerewolfActionTestDefinitionFinding(
    WerewolfActionTestDefinitionFindingSeverity Severity,
    string Code,
    string Message);

public sealed record WerewolfActionTestDefinitionRequest(
    WerewolfInitializedCharacterState Draft,
    int ExpectedDraftVersion,
    string RequestId,
    string AttributeId,
    string AbilityId,
    int Difficulty,
    int? Modifier);

public sealed record WerewolfActionTestDefinitionResult(
    bool Succeeded,
    WerewolfInitializedCharacterState? Draft,
    IReadOnlyList<WerewolfActionTestDefinitionFinding> Findings,
    string? RequestId,
    int? DiceQuantity,
    int? DiceFaces,
    string? AttributeId,
    string? AbilityId,
    int? Difficulty,
    int? Modifier);

public static class WerewolfActionTestDefinitionService
{
    public static WerewolfActionTestDefinitionResult DefineTest(WerewolfActionTestDefinitionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Draft);
        ArgumentNullException.ThrowIfNull(request.RequestId);

        var findings = new List<WerewolfActionTestDefinitionFinding>();

        if (request.Draft.Status != WerewolfCharacterDraftStatus.Completed)
        {
            findings.Add(new WerewolfActionTestDefinitionFinding(
                WerewolfActionTestDefinitionFindingSeverity.Error,
                "CharacterNotCompleted",
                "Action tests can only be defined for completed characters."));
            return new WerewolfActionTestDefinitionResult(false, request.Draft, findings, null, null, null, null, null, null, null);
        }

        if (request.Draft.DraftVersion != request.ExpectedDraftVersion)
        {
            findings.Add(new WerewolfActionTestDefinitionFinding(
                WerewolfActionTestDefinitionFindingSeverity.Error,
                "StaleDraftVersion",
                "Expected draft version does not match current draft version."));
            return new WerewolfActionTestDefinitionResult(false, request.Draft, findings, null, null, null, null, null, null, null);
        }

        if (string.IsNullOrWhiteSpace(request.AttributeId) ||
            !WerewolfAttributeIdentifiers.Supported.Contains(request.AttributeId, StringComparer.Ordinal))
        {
            findings.Add(new WerewolfActionTestDefinitionFinding(
                WerewolfActionTestDefinitionFindingSeverity.Error,
                "InvalidAttribute",
                $"Attribute identifier '{request.AttributeId}' is not recognized."));
            return new WerewolfActionTestDefinitionResult(false, request.Draft, findings, null, null, null, null, null, null, null);
        }

        if (string.IsNullOrWhiteSpace(request.AbilityId) ||
            !WerewolfAbilityIdentifiers.Supported.Contains(request.AbilityId, StringComparer.Ordinal))
        {
            findings.Add(new WerewolfActionTestDefinitionFinding(
                WerewolfActionTestDefinitionFindingSeverity.Error,
                "InvalidAbility",
                $"Ability identifier '{request.AbilityId}' is not recognized."));
            return new WerewolfActionTestDefinitionResult(false, request.Draft, findings, null, null, null, null, null, null, null);
        }

        if (!request.Draft.Attributes.TryGetValue(request.AttributeId, out var attributeRating) || !attributeRating.HasValue)
        {
            findings.Add(new WerewolfActionTestDefinitionFinding(
                WerewolfActionTestDefinitionFindingSeverity.Error,
                "AttributeNotAllocated",
                $"Attribute '{request.AttributeId}' has no allocated rating."));
            return new WerewolfActionTestDefinitionResult(false, request.Draft, findings, null, null, null, null, null, null, null);
        }

        if (!request.Draft.Abilities.TryGetValue(request.AbilityId, out var abilityRating) || !abilityRating.HasValue)
        {
            findings.Add(new WerewolfActionTestDefinitionFinding(
                WerewolfActionTestDefinitionFindingSeverity.Error,
                "AbilityNotAllocated",
                $"Ability '{request.AbilityId}' has no allocated rating."));
            return new WerewolfActionTestDefinitionResult(false, request.Draft, findings, null, null, null, null, null, null, null);
        }

        if (request.Difficulty < 2 || request.Difficulty > 10)
        {
            findings.Add(new WerewolfActionTestDefinitionFinding(
                WerewolfActionTestDefinitionFindingSeverity.Error,
                "InvalidDifficulty",
                "Difficulty must be between 2 and 10 (source line 2781)."));
            return new WerewolfActionTestDefinitionResult(false, request.Draft, findings, null, null, null, null, null, null, null);
        }

        var pool = attributeRating.Value + abilityRating.Value + (request.Modifier ?? 0);
        if (pool < 0)
        {
            pool = 0;
        }

        findings.Add(new WerewolfActionTestDefinitionFinding(
            WerewolfActionTestDefinitionFindingSeverity.Information,
            "ActionTestDefined",
            $"Dice pool of {pool} d10 defined for {request.AttributeId} + {request.AbilityId} at difficulty {request.Difficulty}."));

        return new WerewolfActionTestDefinitionResult(
            true,
            request.Draft,
            findings,
            request.RequestId,
            pool,
            10,
            request.AttributeId,
            request.AbilityId,
            request.Difficulty,
            request.Modifier ?? 0);
    }
}
