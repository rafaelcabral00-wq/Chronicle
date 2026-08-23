namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

using System.Collections.ObjectModel;

public static class WerewolfSocialChallengeCatalog
{
    public static IReadOnlyDictionary<string, WerewolfSocialChallengeDefinition> Entries { get; } =
        new Dictionary<string, WerewolfSocialChallengeDefinition>(StringComparer.Ordinal)
        {
            [WerewolfSocialChallengeIdentifiers.AtracaoAnimal] = new(
                WerewolfSocialChallengeIdentifiers.AtracaoAnimal,
                "Atração Animal",
                WerewolfAttributeIdentifiers.Charisma,
                WerewolfAbilityIdentifiers.PrimalInstinct,
                6,
                false,
                0,
                "Line 3009"),
            [WerewolfSocialChallengeIdentifiers.Credibilidade] = new(
                WerewolfSocialChallengeIdentifiers.Credibilidade,
                "Credibilidade",
                WerewolfAttributeIdentifiers.Manipulation,
                WerewolfAbilityIdentifiers.Subterfuge,
                6,
                false,
                0,
                "Line 3012"),
            [WerewolfSocialChallengeIdentifiers.Defrontacao] = new(
                WerewolfSocialChallengeIdentifiers.Defrontacao,
                "Defrontação",
                WerewolfAttributeIdentifiers.Charisma,
                WerewolfAbilityIdentifiers.Intimidation,
                6,
                true,
                0,
                "Line 3014"),
            [WerewolfSocialChallengeIdentifiers.Engabelacao] = new(
                WerewolfSocialChallengeIdentifiers.Engabelacao,
                "Engambelação",
                WerewolfAttributeIdentifiers.Manipulation,
                WerewolfAbilityIdentifiers.Subterfuge,
                6,
                false,
                0,
                "Line 3018"),
            [WerewolfSocialChallengeIdentifiers.Interrogatorio] = new(
                WerewolfSocialChallengeIdentifiers.Interrogatorio,
                "Interrogatório",
                WerewolfAttributeIdentifiers.Manipulation,
                WerewolfAbilityIdentifiers.Intimidation,
                6,
                false,
                0,
                "Line 3020"),
            [WerewolfSocialChallengeIdentifiers.Intimidacao] = new(
                WerewolfSocialChallengeIdentifiers.Intimidacao,
                "Intimidação",
                WerewolfAttributeIdentifiers.Charisma,
                WerewolfAbilityIdentifiers.Intimidation,
                6,
                false,
                0,
                "Line 3022"),
            [WerewolfSocialChallengeIdentifiers.OratoriaPerformance] = new(
                WerewolfSocialChallengeIdentifiers.OratoriaPerformance,
                "Oratória e Performance",
                WerewolfAttributeIdentifiers.Charisma,
                WerewolfAbilityIdentifiers.Leadership,
                6,
                false,
                0,
                "Line 3024"),
            [WerewolfSocialChallengeIdentifiers.Seducao] = new(
                WerewolfSocialChallengeIdentifiers.Seducao,
                "Sedução",
                WerewolfAttributeIdentifiers.Appearance,
                WerewolfAbilityIdentifiers.Subterfuge,
                6,
                false,
                0,
                "Line 3027")
        };
}
