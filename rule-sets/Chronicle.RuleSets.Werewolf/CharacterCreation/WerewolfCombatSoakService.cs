namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfCombatSoakRequest(
    string RequestId,
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    WerewolfDamageCategory DamageType,
    int IncomingDamage);

public sealed record WerewolfCombatSoakRollDefinition(
    string RequestId,
    int SoakPoolSize,
    int Difficulty,
    bool IsRacialForm,
    bool IsSilver,
    bool SoakBlocked,
    IReadOnlyList<string> Findings);

public sealed record WerewolfCombatSoakRollResult(
    string RequestId,
    IReadOnlyList<int> DiceValues,
    int SoakSuccesses,
    bool IsRacialForm,
    bool IsSilver,
    bool SoakBlocked,
    IReadOnlyList<string> Findings);

public static class WerewolfCombatSoakService
{
    public static WerewolfCombatSoakRollDefinition DefineSoakRoll(WerewolfCombatSoakRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return new WerewolfCombatSoakRollDefinition(string.Empty, 0, 0, false, false, false, ["RequestId is required"]);
        }

        if (request.CurrentState is null)
        {
            return new WerewolfCombatSoakRollDefinition(request.RequestId, 0, 0, false, false, false, ["CurrentState is required"]);
        }

        if (request.ExpectedRuntimeStateVersion < 1)
        {
            return new WerewolfCombatSoakRollDefinition(request.RequestId, 0, 0, false, false, false, ["ExpectedRuntimeStateVersion must be >= 1"]);
        }

        if (request.CurrentState.RuntimeStateVersion != request.ExpectedRuntimeStateVersion)
        {
            return new WerewolfCombatSoakRollDefinition(request.RequestId, 0, 0, false, false, false, ["Version mismatch"]);
        }

        if (!Enum.IsDefined(request.DamageType))
        {
            return new WerewolfCombatSoakRollDefinition(request.RequestId, 0, 0, false, false, false, ["Invalid damage type"]);
        }

        if (request.IncomingDamage <= 0)
        {
            return new WerewolfCombatSoakRollDefinition(request.RequestId, 0, 0, false, false, false, ["Incoming damage must be positive"]);
        }

        var currentForm = request.CurrentState.CurrentForm;
        var birthRace = request.CurrentState.BirthRace;
        var isRacialForm = string.Equals(currentForm, birthRace switch
        {
            WerewolfRaceIdentifiers.Homid => WerewolfFormIdentifiers.Homid,
            WerewolfRaceIdentifiers.Metis => WerewolfFormIdentifiers.Crinos,
            WerewolfRaceIdentifiers.Lupus => WerewolfFormIdentifiers.Lupus,
            _ => string.Empty
        }, StringComparison.Ordinal);

        var isSilver = request.DamageType == WerewolfDamageCategory.Aggravated;
        var canSoak = true;

        if (isSilver)
        {
            if (isRacialForm)
            {
                if (birthRace == WerewolfRaceIdentifiers.Homid || birthRace == WerewolfRaceIdentifiers.Lupus)
                {
                    canSoak = true;
                    findings.Add("Silver soak: racial form allows absorption for homid/lupus.");
                }
                else
                {
                    canSoak = false;
                    findings.Add("Silver soak: racial form does not grant silver absorption for metis.");
                }
            }
            else
            {
                canSoak = false;
                findings.Add("Silver soak: non-racial form requires Gifts or fetishes.");
            }
        }
        else if (request.DamageType != WerewolfDamageCategory.Bashing && !isRacialForm)
        {
            canSoak = true;
            findings.Add("Non-bashing lethal/aggravated soak: allowed in non-racial form.");
        }
        else if (request.DamageType != WerewolfDamageCategory.Bashing && isRacialForm)
        {
            canSoak = false;
            findings.Add("Non-bashing lethal/aggravated soak: not allowed in racial form per source.");
        }

        var vigor = 0;
        if (request.CurrentState.PackageBinding.TryGetValue("attributes", out var attrText) && !string.IsNullOrWhiteSpace(attrText))
        {
            var attributes = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(attrText);
            if (attributes is not null)
            {
                var nullableAttributes = new Dictionary<string, int?>(StringComparer.Ordinal);
                foreach (var kvp in attributes)
                {
                    nullableAttributes[kvp.Key] = kvp.Value;
                }
                vigor = WerewolfEffectiveAttributeService.GetEffectiveAttribute(nullableAttributes, currentForm, WerewolfAttributeIdentifiers.Stamina);
            }
        }

        var toughHideBonus = 0;
        if (request.CurrentState.PackageBinding.TryGetValue("metis-deformities", out var deformitiesText) && !string.IsNullOrWhiteSpace(deformitiesText))
        {
            var deformities = System.Text.Json.JsonSerializer.Deserialize<List<WerewolfMetisDeformityEffect>>(deformitiesText);
            if (deformities is not null && deformities.Any(d => d.Kind == WerewolfMetisDeformityEffectKind.ToughHide))
            {
                toughHideBonus = 1;
            }
        }

        var soakPoolSize = canSoak ? Math.Max(0, vigor + toughHideBonus) : 0;

        var soakBlocked = !canSoak;

        findings.Add($"Soak roll defined: pool {soakPoolSize} (Vigor {vigor} + ToughHide {toughHideBonus}) vs difficulty {DefaultDifficulty}. Can soak: {canSoak}.");

        return new WerewolfCombatSoakRollDefinition(
            request.RequestId,
            soakPoolSize,
            DefaultDifficulty,
            isRacialForm,
            isSilver,
            soakBlocked,
            findings);
    }

    public static WerewolfCombatSoakRollResult InterpretSoakRoll(WerewolfCombatSoakRollDefinition definition, IReadOnlyList<int> diceValues)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(diceValues);

        var findings = new List<string>(definition.Findings);

        if (!definition.IsRacialForm && definition.IsSilver)
        {
            findings.Add("Silver soak blocked: non-racial form without Gifts/fetishes.");
            return new WerewolfCombatSoakRollResult(definition.RequestId, diceValues, 0, definition.IsRacialForm, definition.IsSilver, true, findings);
        }

        if (diceValues.Count != definition.SoakPoolSize)
        {
            findings.Add($"Dice count mismatch: expected {definition.SoakPoolSize}, got {diceValues.Count}.");
            return new WerewolfCombatSoakRollResult(definition.RequestId, diceValues, 0, definition.IsRacialForm, definition.IsSilver, false, findings);
        }

        var successes = 0;
        foreach (var die in diceValues)
        {
            if (die >= definition.Difficulty)
            {
                successes++;
            }
        }

        findings.Add($"Soak roll interpreted: {successes} successes from {diceValues.Count} dice.");

        return new WerewolfCombatSoakRollResult(
            definition.RequestId,
            diceValues,
            successes,
            definition.IsRacialForm,
            definition.IsSilver,
            false,
            findings);
    }

    public static int DefaultDifficulty => 6;
}
