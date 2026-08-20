using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfBirthIdentitySemanticsTests
{
    private static readonly string[] AttributePriorityOrder = ["strength", "dexterity", "stamina", "charisma", "manipulation", "appearance", "perception", "intelligence", "wits"];
    private static readonly string[] AbilityPriorityOrder = ["talents", "skills", "knowledges"];
    private static readonly string[] EmptyGifts = [];
    private static readonly WerewolfCharacterCompletionFinding[] EmptyFindings = [];
    private static readonly string[] EmptyCompletedSteps = [];
    private static readonly WerewolfFreebieLedgerEntry[] EmptyFreebieLedger = [];

    [Fact]
    public void HomidBirthIdentityIsStableAndDistinctFromMetisAndLupus()
    {
        Assert.Equal("homid", WerewolfRaceIdentifiers.Homid);
        Assert.Equal("metis", WerewolfRaceIdentifiers.Metis);
        Assert.Equal("lupus", WerewolfRaceIdentifiers.Lupus);
        Assert.Equal(3, WerewolfRaceIdentifiers.Supported.Count);
        Assert.Equal(WerewolfRaceIdentifiers.Supported, new[] { WerewolfRaceIdentifiers.Homid, WerewolfRaceIdentifiers.Lupus, WerewolfRaceIdentifiers.Metis });
    }

    [Fact]
    public void BirthIdentityKeysAreLanguageNeutralAndWhitespaceFree()
    {
        Assert.All(WerewolfRaceIdentifiers.Supported, key =>
        {
            Assert.DoesNotContain(key, c => char.IsWhiteSpace(c));
            Assert.Equal(key, key.ToLowerInvariant());
        });
    }

    [Fact]
    public void MetisIsTheCanonicalTechnicalKeyNotImpuro()
    {
        Assert.Equal("metis", WerewolfRaceIdentifiers.Metis);
        Assert.DoesNotContain(WerewolfRaceIdentifiers.Supported, key => string.Equals(key, "impuro", StringComparison.Ordinal));
        Assert.DoesNotContain(WerewolfRaceIdentifiers.Supported, key => string.Equals(key, "impura", StringComparison.Ordinal));
    }

    [Fact]
    public void BirthIdentityIsNotCurrentForm()
    {
        var draft = Draft() with { Race = WerewolfRaceIdentifiers.Metis };

        Assert.Equal(WerewolfRaceIdentifiers.Metis, draft.Race);
        Assert.Null(draft.MetisDeformity);

        Assert.DoesNotContain("Crinos", draft.RequiredNextSteps ?? []);
        Assert.DoesNotContain("Glabro", draft.RequiredNextSteps ?? []);
        Assert.DoesNotContain("Hispo", draft.RequiredNextSteps ?? []);
        Assert.DoesNotContain("Hominídea", draft.RequiredNextSteps ?? []);
        Assert.DoesNotContain("Lupina", draft.RequiredNextSteps ?? []);
    }

    [Fact]
    public void RedTalonsEligibilityUsesBirthIdentityNotCurrentForm()
    {
        var lupusDraft = Draft() with { Race = WerewolfRaceIdentifiers.Lupus };
        var eligibility = WerewolfTribeEligibilityService.CheckEligibility(
            new WerewolfTribeEligibilityRequest(WerewolfTribeIdentifiers.RedTalons, lupusDraft.Race, lupusDraft.Backgrounds));

        Assert.DoesNotContain(eligibility.Findings, f => f.Code == WerewolfTribeEligibilityErrorCode.RaceBreedIneligible);
    }

    [Fact]
    public void HomidCannotAccessRedTalonsViaBirthIdentity()
    {
        var homidDraft = Draft() with { Race = WerewolfRaceIdentifiers.Homid };
        var eligibility = WerewolfTribeEligibilityService.CheckEligibility(
            new WerewolfTribeEligibilityRequest(WerewolfTribeIdentifiers.RedTalons, homidDraft.Race, homidDraft.Backgrounds));

        Assert.Contains(eligibility.Findings, f => f.Code == WerewolfTribeEligibilityErrorCode.RaceBreedIneligible);
    }

    [Fact]
    public void MetisDeformityEligibilityUsesBirthIdentity()
    {
        var metisDraft = Draft() with { Race = WerewolfRaceIdentifiers.Metis };
        var result = WerewolfMetisDeformitySelectionService.SelectDeformity(
            new WerewolfMetisDeformitySelectionRequest(metisDraft, metisDraft.DraftVersion, WerewolfMetisDeformityIdentifiers.Horns));

        Assert.True(result.Succeeded);
        Assert.Equal(WerewolfMetisDeformityIdentifiers.Horns, result.Draft?.MetisDeformity);
    }

    [Fact]
    public void HomidCannotAccessMetisDeformityViaBirthIdentity()
    {
        var homidDraft = Draft() with { Race = WerewolfRaceIdentifiers.Homid };
        var result = WerewolfMetisDeformitySelectionService.SelectDeformity(
            new WerewolfMetisDeformitySelectionRequest(homidDraft, homidDraft.DraftVersion, WerewolfMetisDeformityIdentifiers.Horns));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == WerewolfMetisDeformitySelectionErrorCode.RaceNotMetis);
    }

    [Fact]
    public void BirthIdentityPreservesThroughSnapshot()
    {
        var snapshot = BuildSnapshot(WerewolfRaceIdentifiers.Metis);

        Assert.Equal(WerewolfRaceIdentifiers.Metis, snapshot.Race);
    }

    private static WerewolfInitializedCharacterState Draft()
    {
        return WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity("draft-001"), 1);
    }

    private static WerewolfCharacterSnapshot BuildSnapshot(string race)
    {
        return new WerewolfCharacterSnapshot(
            "draft-1",
            1,
            WerewolfCharacterDraftStatus.Completed,
            race,
            WerewolfAuspiceIdentifiers.Ragabash,
            WerewolfTribeIdentifiers.BoneGnawers,
            WerewolfMetisDeformityIdentifiers.Horns,
            "gift.race.metis.create-element",
            "gift.auspice.ragabash.open-seal",
            "gift.tribe.bone-gnawers.cooking",
            Array.AsReadOnly(AttributePriorityOrder),
            new Dictionary<string, int>(StringComparer.Ordinal),
            Array.AsReadOnly(AbilityPriorityOrder),
            new Dictionary<string, int>(StringComparer.Ordinal),
            new Dictionary<string, int?>(StringComparer.Ordinal),
            new Dictionary<string, int?>(StringComparer.Ordinal),
            new Dictionary<string, int?>(StringComparer.Ordinal),
            Array.AsReadOnly(EmptyGifts),
            new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfCharacterResourceIdentifiers.RagePermanent] = 1,
                [WerewolfCharacterResourceIdentifiers.RageCurrent] = 1,
                [WerewolfCharacterResourceIdentifiers.GnosisPermanent] = 1,
                [WerewolfCharacterResourceIdentifiers.GnosisCurrent] = 1,
                [WerewolfCharacterResourceIdentifiers.WillpowerPermanent] = 1,
                [WerewolfCharacterResourceIdentifiers.WillpowerCurrent] = 1
            },
            new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfRenownIdentifiers.GloryPermanent] = 0,
                [WerewolfRenownIdentifiers.GloryCurrent] = 0,
                [WerewolfRenownIdentifiers.HonorPermanent] = 0,
                [WerewolfRenownIdentifiers.HonorCurrent] = 0,
                [WerewolfRenownIdentifiers.WisdomPermanent] = 0,
                [WerewolfRenownIdentifiers.WisdomCurrent] = 0
            },
            null,
            null,
            null,
            new Dictionary<string, string?>(StringComparer.Ordinal),
            new Dictionary<string, string>(StringComparer.Ordinal),
            Array.AsReadOnly(EmptyFindings),
            Array.AsReadOnly(EmptyCompletedSteps),
            "fingerprint",
            Array.AsReadOnly(EmptyFreebieLedger),
            0,
            0);
    }
}
