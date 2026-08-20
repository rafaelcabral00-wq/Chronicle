using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfRuntimeCharacterStateTests
{
    private static readonly string[] AttributePriorityOrder = ["strength", "dexterity", "stamina", "charisma", "manipulation", "appearance", "perception", "intelligence", "wits"];
    private static readonly string[] AbilityPriorityOrder = ["talents", "skills", "knowledges"];
    private static readonly string[] EmptyGifts = [];
    private static readonly WerewolfCharacterCompletionFinding[] EmptyFindings = [];
    private static readonly string[] EmptyCompletedSteps = [];
    private static readonly WerewolfFreebieLedgerEntry[] EmptyFreebieLedger = [];

    [Fact]
    public void FromSnapshotWithWeakImmuneSystemSetsHealthTrackFlag()
    {
        var snapshot = BuildSnapshot(WerewolfMetisDeformityIdentifiers.WeakImmuneSystem);

        var state = WerewolfRuntimeCharacterState.FromSnapshot(snapshot);

        Assert.NotNull(state.HealthTrack);
        Assert.True(state.HealthTrack.HasWeakenedImmuneSystem);
        Assert.Equal(WerewolfHealthLevelName.Machucado, state.HealthTrack.CurrentLevel);
    }

    [Fact]
    public void FromSnapshotWithoutWeakImmuneSystemLeavesHealthTrackNormal()
    {
        var snapshot = BuildSnapshot(null);

        var state = WerewolfRuntimeCharacterState.FromSnapshot(snapshot);

        Assert.NotNull(state.HealthTrack);
        Assert.False(state.HealthTrack.HasWeakenedImmuneSystem);
        Assert.Equal(WerewolfHealthLevelName.Escoriado, state.HealthTrack.CurrentLevel);
    }

    [Fact]
    public void FromSnapshotWithOtherDeformityLeavesHealthTrackNormal()
    {
        var snapshot = BuildSnapshot(WerewolfMetisDeformityIdentifiers.Horns);

        var state = WerewolfRuntimeCharacterState.FromSnapshot(snapshot);

        Assert.NotNull(state.HealthTrack);
        Assert.False(state.HealthTrack.HasWeakenedImmuneSystem);
        Assert.Equal(WerewolfHealthLevelName.Escoriado, state.HealthTrack.CurrentLevel);
    }

    [Fact]
    public void FromSnapshotWithMetisRaceInitializesCrinosForm()
    {
        var snapshot = BuildSnapshot(null, WerewolfRaceIdentifiers.Metis);

        var state = WerewolfRuntimeCharacterState.FromSnapshot(snapshot);

        Assert.Equal(WerewolfFormIdentifiers.Crinos, state.CurrentForm);
    }

    [Fact]
    public void FromSnapshotWithHomidRaceInitializesHomidForm()
    {
        var snapshot = BuildSnapshot(null, WerewolfRaceIdentifiers.Homid);

        var state = WerewolfRuntimeCharacterState.FromSnapshot(snapshot);

        Assert.Equal(WerewolfFormIdentifiers.Homid, state.CurrentForm);
    }

    [Fact]
    public void FromSnapshotWithLupusRaceInitializesLupusForm()
    {
        var snapshot = BuildSnapshot(null, WerewolfRaceIdentifiers.Lupus);

        var state = WerewolfRuntimeCharacterState.FromSnapshot(snapshot);

        Assert.Equal(WerewolfFormIdentifiers.Lupus, state.CurrentForm);
    }

    [Fact]
    public void BirthRaceRemainsImmutableRegardlessOfForm()
    {
        var snapshot = BuildSnapshot(null, WerewolfRaceIdentifiers.Metis);

        var state = WerewolfRuntimeCharacterState.FromSnapshot(snapshot);

        Assert.Equal(WerewolfRaceIdentifiers.Metis, state.BirthRace);
        Assert.Equal(WerewolfFormIdentifiers.Crinos, state.CurrentForm);
    }

    private static WerewolfCharacterSnapshot BuildSnapshot(string? metisDeformity, string race = WerewolfRaceIdentifiers.Metis)
    {
        return new WerewolfCharacterSnapshot(
            "draft-1",
            1,
            WerewolfCharacterDraftStatus.Completed,
            race,
            WerewolfAuspiceIdentifiers.Ragabash,
            WerewolfTribeIdentifiers.BoneGnawers,
            metisDeformity,
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
            0,
            WerewolfFormIdentifiers.Homid);
    }
}
