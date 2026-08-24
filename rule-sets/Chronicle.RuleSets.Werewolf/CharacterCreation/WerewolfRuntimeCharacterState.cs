namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfRuntimeCharacterState(
    string PackageId,
    string PackageVersion,
    string DraftId,
    int RuntimeStateVersion,
    IReadOnlyDictionary<string, string> PackageBinding,
    int RagePermanent,
    int RageCurrent,
    int GnosisPermanent,
    int GnosisCurrent,
    int WillpowerPermanent,
    int WillpowerCurrent,
    int GloryPermanent,
    int GloryCurrent,
    int HonorPermanent,
    int HonorCurrent,
    int WisdomPermanent,
    int WisdomCurrent,
    string BirthRace,
    WerewolfHealthTrack? HealthTrack,
    string CurrentForm = "",
    IReadOnlyList<WerewolfCondition> Conditions = null!,
    WerewolfFrenzyState? FrenzyState = null,
    IReadOnlyList<WerewolfActiveGiftEffect> ActiveGiftEffects = null!,
    IReadOnlyList<string> KnownGiftKeys = null!,
    IReadOnlyDictionary<string, int> SceneGiftUsage = null!,
    string CurrentSceneToken = "",
    IReadOnlyList<string> ActivatedGiftKeys = null!)
{
    public static WerewolfRuntimeCharacterState FromSnapshot(WerewolfCharacterSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var resources = snapshot.Resources;
        if (resources is null)
        {
            throw new ArgumentException("Completed character snapshot must contain Resources.", nameof(snapshot));
        }

        var renown = snapshot.Renown;
        if (renown is null)
        {
            throw new ArgumentException("Completed character snapshot must contain Renown.", nameof(snapshot));
        }

        if (!resources.TryGetValue(WerewolfCharacterResourceIdentifiers.RagePermanent, out var ragePermanent) || ragePermanent is null)
        {
            throw new ArgumentException("Snapshot is missing Rage permanent value.", nameof(snapshot));
        }

        if (!resources.TryGetValue(WerewolfCharacterResourceIdentifiers.RageCurrent, out var rageCurrent) || rageCurrent is null)
        {
            throw new ArgumentException("Snapshot is missing Rage current value.", nameof(snapshot));
        }

        if (!resources.TryGetValue(WerewolfCharacterResourceIdentifiers.GnosisPermanent, out var gnosisPermanent) || gnosisPermanent is null)
        {
            throw new ArgumentException("Snapshot is missing Gnosis permanent value.", nameof(snapshot));
        }

        if (!resources.TryGetValue(WerewolfCharacterResourceIdentifiers.GnosisCurrent, out var gnosisCurrent) || gnosisCurrent is null)
        {
            throw new ArgumentException("Snapshot is missing Gnosis current value.", nameof(snapshot));
        }

        if (!resources.TryGetValue(WerewolfCharacterResourceIdentifiers.WillpowerPermanent, out var willpowerPermanent) || willpowerPermanent is null)
        {
            throw new ArgumentException("Snapshot is missing Willpower permanent value.", nameof(snapshot));
        }

        if (!resources.TryGetValue(WerewolfCharacterResourceIdentifiers.WillpowerCurrent, out var willpowerCurrent) || willpowerCurrent is null)
        {
            throw new ArgumentException("Snapshot is missing Willpower current value.", nameof(snapshot));
        }

        var gloryPermanent = renown.GetValueOrDefault(WerewolfRenownIdentifiers.GloryPermanent, 0);
        var gloryCurrent = renown.GetValueOrDefault(WerewolfRenownIdentifiers.GloryCurrent, 0);
        var honorPermanent = renown.GetValueOrDefault(WerewolfRenownIdentifiers.HonorPermanent, 0);
        var honorCurrent = renown.GetValueOrDefault(WerewolfRenownIdentifiers.HonorCurrent, 0);
        var wisdomPermanent = renown.GetValueOrDefault(WerewolfRenownIdentifiers.WisdomPermanent, 0);
        var wisdomCurrent = renown.GetValueOrDefault(WerewolfRenownIdentifiers.WisdomCurrent, 0);

        var race = snapshot.Race;
        if (string.IsNullOrWhiteSpace(race))
        {
            throw new ArgumentException("Snapshot is missing Race value.", nameof(snapshot));
        }

        var currentForm = race switch
        {
            WerewolfRaceIdentifiers.Homid => WerewolfFormIdentifiers.Homid,
            WerewolfRaceIdentifiers.Metis => WerewolfFormIdentifiers.Crinos,
            WerewolfRaceIdentifiers.Lupus => WerewolfFormIdentifiers.Lupus,
            _ => throw new ArgumentException($"Unknown birth race '{race}' for form initialization.", nameof(snapshot))
        };

        var knownGiftKeys = snapshot.Gifts ?? [];
        if (knownGiftKeys.Count == 0)
        {
            var fallback = new List<string>();
            if (!string.IsNullOrWhiteSpace(snapshot.RaceGift))
            {
                fallback.Add(snapshot.RaceGift);
            }
            if (!string.IsNullOrWhiteSpace(snapshot.AuspiceGift))
            {
                fallback.Add(snapshot.AuspiceGift);
            }
            if (!string.IsNullOrWhiteSpace(snapshot.TribeGift))
            {
                fallback.Add(snapshot.TribeGift);
            }
            knownGiftKeys = fallback;
        }

        return new WerewolfRuntimeCharacterState(
            snapshot.PackageBinding.TryGetValue("packageId", out var pkgId) ? pkgId : string.Empty,
            snapshot.PackageBinding.TryGetValue("packageVersion", out var pkgVer) ? pkgVer : string.Empty,
            snapshot.DraftId,
            1,
            snapshot.PackageBinding,
            ragePermanent.Value,
            rageCurrent.Value,
            gnosisPermanent.Value,
            gnosisCurrent.Value,
            willpowerPermanent.Value,
            willpowerCurrent.Value,
            gloryPermanent ?? 0,
            gloryCurrent ?? 0,
            honorPermanent ?? 0,
            honorCurrent ?? 0,
            wisdomPermanent ?? 0,
            wisdomCurrent ?? 0,
            race,
            WerewolfHealthTrackComputer.Compute(
                [],
                hasWeakenedImmuneSystem: StringComparer.Ordinal.Equals(snapshot.MetisDeformity, WerewolfMetisDeformityIdentifiers.WeakImmuneSystem),
                lastRegenerationTurn: -1),
            currentForm,
            Conditions: [],
            FrenzyState: null,
            ActiveGiftEffects: [],
            KnownGiftKeys: knownGiftKeys,
            SceneGiftUsage: new Dictionary<string, int>(StringComparer.Ordinal),
            CurrentSceneToken: string.Empty);
    }
}
