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
                1,
                null,
                null,
                true,
                WerewolfAbilityIdentifiers.Firearms,
                1,
                true,
                false),

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
                1,
                null,
                null,
                true,
                WerewolfAbilityIdentifiers.Athletics,
                null,
                false,
                false),

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
                1,
                null,
                null,
                false,
                null,
                null,
                false,
                false),

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
                1,
                null,
                null,
                false,
                null,
                null,
                false,
                false),

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
                1,
                null,
                null,
                false,
                null,
                null,
                false,
                false),

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
                1,
                null,
                null,
                false,
                null,
                null,
                false,
                false),

            [WerewolfCombatIdentifiers.Bow] = new WerewolfCombatAttackDefinition(
                WerewolfCombatIdentifiers.Bow,
                "Line 3117",
                WerewolfAttributeIdentifiers.Dexterity,
                WerewolfAbilityIdentifiers.Athletics,
                6,
                1,
                "Variable",
                WerewolfDamageCategory.Lethal.ToString(),
                false,
                [WerewolfFormIdentifiers.Homid, WerewolfFormIdentifiers.Glabro, WerewolfFormIdentifiers.Crinos],
                false,
                null,
                1,
                null,
                null,
                true,
                WerewolfAbilityIdentifiers.Athletics,
                null,
                false,
                false)
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
