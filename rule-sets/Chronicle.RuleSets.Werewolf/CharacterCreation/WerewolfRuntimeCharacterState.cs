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
    WerewolfHealthTrack? HealthTrack)
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
            WerewolfHealthTrackComputer.Compute([], lastRegenerationTurn: -1));
    }
}
