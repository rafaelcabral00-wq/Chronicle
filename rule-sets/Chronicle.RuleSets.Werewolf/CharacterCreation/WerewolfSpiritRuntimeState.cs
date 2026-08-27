namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfSpiritRuntimeState(
    string SpiritId,
    string CategoryKey,
    int WillpowerPermanent,
    int WillpowerCurrent,
    int RagePermanent,
    int RageCurrent,
    int GnosisPermanent,
    int GnosisCurrent,
    int EssenceCurrent,
    bool IsMaterialized,
    IReadOnlyList<string> KnownCharmKeys,
    int StateVersion)
{
    public int EssencePermanent => WillpowerPermanent + RagePermanent + GnosisPermanent;

    public static WerewolfSpiritRuntimeState Create(
        string spiritId,
        string categoryKey,
        int willpowerPermanent,
        int ragePermanent,
        int gnosisPermanent,
        IReadOnlyList<string> knownCharmKeys)
    {
        var essence = willpowerPermanent + ragePermanent + gnosisPermanent;
        return new WerewolfSpiritRuntimeState(
            spiritId,
            categoryKey,
            willpowerPermanent,
            willpowerPermanent,
            ragePermanent,
            ragePermanent,
            gnosisPermanent,
            gnosisPermanent,
            essence,
            false,
            knownCharmKeys,
            1);
    }
}
