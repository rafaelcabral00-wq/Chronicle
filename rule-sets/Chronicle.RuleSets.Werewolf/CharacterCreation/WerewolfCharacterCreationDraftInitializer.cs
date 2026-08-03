using System.Globalization;
using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed class WerewolfCharacterCreationDraftInitializer
{
    private readonly IWerewolfCharacterDraftIdentitySource identitySource;

    public WerewolfCharacterCreationDraftInitializer(IWerewolfCharacterDraftIdentitySource identitySource)
    {
        this.identitySource = identitySource;
    }

    public WerewolfCreateCharacterResultPayload Initialize(WerewolfCreateCharacterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return Invalid("MissingRequestId", "Create-character request requires a deterministic request id.");
        }

        var identity = identitySource.CreateDraftIdentity(request);
        if (string.IsNullOrWhiteSpace(identity.Value))
        {
            return Invalid("InvalidDraftIdentity", "Chronicle identity source returned an invalid draft identity.");
        }

        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(identity, 1);

        return new WerewolfCreateCharacterResultPayload(
            true,
            draft,
            [new WerewolfCharacterInitializationFinding(WerewolfCharacterInitializationFindingSeverity.Information, "DraftInitialized", "Character creation draft initialized.")]);
    }

    private static WerewolfCreateCharacterResultPayload Invalid(string code, string message)
    {
        return new WerewolfCreateCharacterResultPayload(
            false,
            null,
            [new WerewolfCharacterInitializationFinding(WerewolfCharacterInitializationFindingSeverity.Error, code, message)]);
    }

}

public static class WerewolfCharacterCreationDraftFactory
{
    private static readonly string[] AttributeKeys = ["mental", "physical", "social"];
    private static readonly string[] AbilityKeys =
    [
        "character.ability.alertness",
        "character.ability.athletics",
        "character.ability.brawl",
        "character.ability.computer",
        "character.ability.drive",
        "character.ability.empathy",
        "character.ability.etiquette",
        "character.ability.expression",
        "character.ability.intimidation",
        "character.ability.investigation",
        "character.ability.law",
        "character.ability.leadership",
        "character.ability.occult",
        "character.ability.performance",
        "character.ability.politics",
        "character.ability.stealth",
        "character.ability.subterfuge",
        "character.ability.survival"
    ];
    private static readonly string[] BackgroundKeys = ["background-selection"];
    private static readonly string[] ResourceKeys = ["gnosis", "rage", "willpower"];
    private static readonly string[] NarrativeFieldKeys = ["character-concept", "character-goals", "character-relationships", "name"];
    private static readonly string[] RequiredNextSteps = ["select-race", "select-auspice", "select-tribe", "allocate-attributes", "allocate-abilities", "select-backgrounds", "select-initial-gifts", "review-resources", "add-narrative-fields"];

    public static WerewolfInitializedCharacterState CreateInitializedDraft(WerewolfCharacterDraftIdentity identity, int draftVersion)
    {
        return new WerewolfInitializedCharacterState(
            identity,
            WerewolfCharacterDraftStatus.Initialized,
            draftVersion,
            Race: null,
            Auspice: null,
            Tribe: null,
            MetisDeformity: null,
            RaceGift: null,
            AuspiceGift: null,
            TribeGift: null,
            AttributePriorityOrder: [],
            AttributeBudgets: new ReadOnlyDictionary<string, int>(new Dictionary<string, int>(StringComparer.Ordinal)),
            AbilityPriorityOrder: [],
            AbilityBudgets: new ReadOnlyDictionary<string, int>(new Dictionary<string, int>(StringComparer.Ordinal)),
            Attributes: UnsetDictionary(AttributeKeys),
            Abilities: UnsetDictionary(AbilityKeys),
            Backgrounds: UnsetDictionary(BackgroundKeys),
            Gifts: [],
            Resources: UnsetDictionary(ResourceKeys),
            NarrativeFields: NarrativeDictionary(NarrativeFieldKeys),
            RequiredNextSteps: Array.AsReadOnly(RequiredNextSteps.ToArray()),
            DisabledCapabilities: new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["additional-gift-purchase"] = "disabled",
                ["runtime-gift-execution"] = "disabled"
            }));
    }

    private static ReadOnlyDictionary<string, int?> UnsetDictionary(IEnumerable<string> keys)
    {
        var values = keys
            .Order(StringComparer.Ordinal)
            .ToDictionary(key => key, _ => (int?)null, StringComparer.Ordinal);
        return new ReadOnlyDictionary<string, int?>(values);
    }

    private static ReadOnlyDictionary<string, string?> NarrativeDictionary(IEnumerable<string> keys)
    {
        var values = keys
            .Order(StringComparer.Ordinal)
            .ToDictionary(key => key, _ => (string?)null, StringComparer.Ordinal);
        return new ReadOnlyDictionary<string, string?>(values);
    }
}

public sealed class InMemoryWerewolfCharacterDraftIdentitySource : IWerewolfCharacterDraftIdentitySource
{
    private int next;

    public WerewolfCharacterDraftIdentity CreateDraftIdentity(WerewolfCreateCharacterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var value = Interlocked.Increment(ref next).ToString("D8", CultureInfo.InvariantCulture);
        return new WerewolfCharacterDraftIdentity($"werewolf-draft-{value}");
    }
}
