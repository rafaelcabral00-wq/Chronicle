namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfCombatAttackDefinitionService
{
    public static IReadOnlyDictionary<string, WerewolfCombatAttackDefinition> Entries { get; } =
        new Dictionary<string, WerewolfCombatAttackDefinition>(StringComparer.Ordinal)
        {
            [WerewolfCombatIdentifiers.Firearm] = new WerewolfCombatAttackDefinition(
                WerewolfCombatIdentifiers.Firearm,
                "Line 3081",
                WerewolfAttributeIdentifiers.Dexterity,
                WerewolfAbilityIdentifiers.Firearms,
                6,
                null,
                "Variable",
                WerewolfDamageCategory.Lethal.ToString(),
                false,
                [WerewolfFormIdentifiers.Homid, WerewolfFormIdentifiers.Glabro, WerewolfFormIdentifiers.Crinos],
                false,
                null,
                1),

            [WerewolfCombatIdentifiers.Thrown] = new WerewolfCombatAttackDefinition(
                WerewolfCombatIdentifiers.Thrown,
                "Line 3082",
                WerewolfAttributeIdentifiers.Dexterity,
                WerewolfAbilityIdentifiers.Athletics,
                6,
                null,
                "Variable",
                WerewolfDamageCategory.Lethal.ToString(),
                false,
                [WerewolfFormIdentifiers.Homid, WerewolfFormIdentifiers.Glabro, WerewolfFormIdentifiers.Crinos],
                false,
                null,
                1),

            [WerewolfCombatIdentifiers.MeleeWeapon] = new WerewolfCombatAttackDefinition(
                WerewolfCombatIdentifiers.MeleeWeapon,
                "Line 3083",
                WerewolfAttributeIdentifiers.Dexterity,
                WerewolfAbilityIdentifiers.Melee,
                6,
                null,
                "Variable",
                WerewolfDamageCategory.Lethal.ToString(),
                true,
                [WerewolfFormIdentifiers.Homid, WerewolfFormIdentifiers.Glabro, WerewolfFormIdentifiers.Crinos],
                false,
                null,
                1),

            [WerewolfCombatIdentifiers.Brawl] = new WerewolfCombatAttackDefinition(
                WerewolfCombatIdentifiers.Brawl,
                "Line 3084",
                WerewolfAttributeIdentifiers.Dexterity,
                WerewolfAbilityIdentifiers.Brawl,
                6,
                null,
                "Strength",
                WerewolfDamageCategory.Bashing.ToString(),
                false,
                [WerewolfFormIdentifiers.Homid, WerewolfFormIdentifiers.Glabro, WerewolfFormIdentifiers.Crinos, WerewolfFormIdentifiers.Hispo, WerewolfFormIdentifiers.Lupus],
                false,
                null,
                1),

            [WerewolfCombatIdentifiers.Claw] = new WerewolfCombatAttackDefinition(
                WerewolfCombatIdentifiers.Claw,
                "Lines 3137-3140",
                WerewolfAttributeIdentifiers.Dexterity,
                WerewolfAbilityIdentifiers.Brawl,
                6,
                null,
                "Strength + 1",
                WerewolfDamageCategory.Aggravated.ToString(),
                true,
                [WerewolfFormIdentifiers.Crinos, WerewolfFormIdentifiers.Hispo],
                true,
                "claw",
                1),

            [WerewolfCombatIdentifiers.Bite] = new WerewolfCombatAttackDefinition(
                WerewolfCombatIdentifiers.Bite,
                "Lines 3129-3132",
                WerewolfAttributeIdentifiers.Dexterity,
                WerewolfAbilityIdentifiers.Brawl,
                5,
                null,
                "Strength + 1",
                WerewolfDamageCategory.Aggravated.ToString(),
                true,
                [WerewolfFormIdentifiers.Crinos, WerewolfFormIdentifiers.Lupus, WerewolfFormIdentifiers.Glabro],
                true,
                "bite",
                1)
        };

    public static WerewolfCombatAttackDefinition ResolveAttack(string attackId)
    {
        if (string.IsNullOrWhiteSpace(attackId))
        {
            throw new ArgumentException("AttackId is required.", nameof(attackId));
        }

        return Entries.TryGetValue(attackId, out var definition)
            ? definition
            : throw new ArgumentException($"Unknown attack definition: {attackId}", nameof(attackId));
    }
}
