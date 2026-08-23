namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfSocialTargetContext(
    int? TargetWillpower = null,
    int? TargetRaciocinio = null,
    int? TargetInteligencia = null,
    int? TargetRage = null,
    bool IsGarouTarget = false,
    bool IsHumanTarget = false,
    bool HasPriorInterest = true,
    bool IsAffectedByGarouCurse = false,
    bool IsTruthBeingTold = false,
    int TruthLevel = 0,
    int? CrowdDispositionBonus = null,
    int? CharacterRankValue = null,
    bool UsesPhysicalPosture = false);
