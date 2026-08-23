namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfBestaInteriorService
{
    public static int ComputeSocialDicePenalty(int ragePermanent, int willpowerPermanent)
    {
        if (ragePermanent <= willpowerPermanent)
        {
            return 0;
        }

        return ragePermanent - willpowerPermanent;
    }

    public static bool IsSocialTest(string attributeId, string abilityId)
    {
        var socialAttributes = new[]
        {
            WerewolfAttributeIdentifiers.Charisma,
            WerewolfAttributeIdentifiers.Manipulation,
            WerewolfAttributeIdentifiers.Appearance
        };

        var socialAbilities = new[]
        {
            WerewolfAbilityIdentifiers.Empathy,
            WerewolfAbilityIdentifiers.Expression,
            WerewolfAbilityIdentifiers.Intimidation,
            WerewolfAbilityIdentifiers.Subterfuge,
            WerewolfAbilityIdentifiers.Leadership,
            WerewolfAbilityIdentifiers.Streetwise,
            WerewolfAbilityIdentifiers.Performance,
            WerewolfAbilityIdentifiers.Etiquette,
            WerewolfAbilityIdentifiers.Politics
        };

        return socialAttributes.Contains(attributeId, StringComparer.OrdinalIgnoreCase) ||
               socialAbilities.Contains(abilityId, StringComparer.OrdinalIgnoreCase);
    }
}
