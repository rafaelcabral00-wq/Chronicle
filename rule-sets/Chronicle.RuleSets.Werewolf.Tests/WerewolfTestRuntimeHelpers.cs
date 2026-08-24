using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;

namespace Chronicle.RuleSets.Werewolf.Tests;

public static class WerewolfTestRuntimeHelpers
{
    public static RuleSetRuntimeRegistry RegisteredRuntimeRegistry()
    {
        return RuleSetRuntimeRegistrationService.Register(new RuleSetRuntimeRegistrationRequest(RegisteredCatalog(), [new WerewolfReferenceRuntime()])).Registry;
    }

    public static RuleSetRuntimeRegistry RegisteredRuntimeRegistry(IWerewolfCharacterDraftIdentitySource identitySource)
    {
        return RuleSetRuntimeRegistrationService.Register(new RuleSetRuntimeRegistrationRequest(RegisteredCatalog(), [new WerewolfReferenceRuntime(identitySource)])).Registry;
    }

    public static RuleSetPackageCatalog RegisteredCatalog()
    {
        var discovery = RuleSetPackageSourceDiscoveryService.Discover(new RuleSetPackageSourceDiscoveryRequest([RuleSetsRoot()]));
        var registration = RuleSetPackageRegistrationService.Register(new RuleSetPackageRegistrationRequest(discovery.ValidatedPackages, 1));
        return registration.Catalog;
    }

    public static WerewolfInitializedCharacterState BuildCompletedDraft(string race, string auspice, string tribe)
    {
        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(
            new WerewolfCharacterDraftIdentity("draft-" + Guid.NewGuid().ToString("N", System.Globalization.CultureInfo.InvariantCulture)), 1);

        return draft with
        {
            Status = WerewolfCharacterDraftStatus.Completed,
            Race = race,
            Auspice = auspice,
            Tribe = tribe,
            MetisDeformity = null,
            RaceGift = WerewolfGiftIdentifiers.HomidMasterOfFire,
            AuspiceGift = WerewolfGiftIdentifiers.RagabashOpenSeal,
            TribeGift = WerewolfGiftIdentifiers.GlassWalkersControlSimpleMachine,
            Attributes = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfAttributeIdentifiers.Strength] = 3,
                [WerewolfAttributeIdentifiers.Dexterity] = 2,
                [WerewolfAttributeIdentifiers.Stamina] = 3,
                [WerewolfAttributeIdentifiers.Charisma] = 2,
                [WerewolfAttributeIdentifiers.Manipulation] = 2,
                [WerewolfAttributeIdentifiers.Appearance] = 2,
                [WerewolfAttributeIdentifiers.Perception] = 3,
                [WerewolfAttributeIdentifiers.Intelligence] = 2,
                [WerewolfAttributeIdentifiers.Wits] = 3
            },
            Abilities = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfAbilityIdentifiers.Athletics] = 3,
                [WerewolfAbilityIdentifiers.Brawl] = 2
            },
            Backgrounds = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfBackgroundIdentifiers.Allies] = 1,
                [WerewolfBackgroundIdentifiers.Ancestors] = 0,
                [WerewolfBackgroundIdentifiers.Contacts] = 1,
                [WerewolfBackgroundIdentifiers.Fetish] = 0,
                [WerewolfBackgroundIdentifiers.Kinfolk] = 0,
                [WerewolfBackgroundIdentifiers.Mentor] = 1,
                [WerewolfBackgroundIdentifiers.PureBreed] = 0,
                [WerewolfBackgroundIdentifiers.Resources] = 1,
                [WerewolfBackgroundIdentifiers.Rites] = 1
            },
            Resources = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                ["gnosis"] = 1,
                ["rage"] = 1,
                ["willpower"] = 3
            },
            Rank = "cliath",
            RankValue = 1,
            IdentityName = "Test Character",
            RequiredNextSteps = []
        };
    }

    private static string RuleSetsRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Chronicle.sln")))
            {
                return Path.Combine(directory.FullName, "rule-sets");
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not find repository root from test base directory.");
    }

    public sealed class TestIdentitySource(string identity) : IWerewolfCharacterDraftIdentitySource
    {
        public WerewolfCharacterDraftIdentity CreateDraftIdentity(WerewolfCreateCharacterRequest request)
        {
            return new WerewolfCharacterDraftIdentity(identity);
        }
    }
}
