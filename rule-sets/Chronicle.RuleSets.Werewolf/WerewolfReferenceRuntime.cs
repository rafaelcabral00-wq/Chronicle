using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using System.Collections.Generic;
using System.Linq;

namespace Chronicle.RuleSets.Werewolf;

public sealed class WerewolfReferenceRuntime : IRuleSetRuntime
{
    public const string CreateCharacterOperation = "character-creation.create-character";
    public const string SelectRaceOperation = "character-creation.select-race";
    public const string SelectAuspiceOperation = "character-creation.select-auspice";
    public const string SelectTribeOperation = "character-creation.select-tribe";
    public const string SelectMetisDeformityOperation = "character-creation.select-metis-deformity";
    public const string SelectRaceGiftOperation = "character-creation.select-race-gift";
    public const string SelectAuspiceGiftOperation = "character-creation.select-auspice-gift";
    public const string SelectTribeGiftOperation = "character-creation.select-tribe-gift";
    public const string SelectAttributePrioritiesOperation = "character-creation.select-attribute-priorities";
    public const string AllocateAttributesOperation = "character-creation.allocate-attributes";
    public const string SelectAbilityPrioritiesOperation = "character-creation.select-ability-priorities";
    public const string AllocateAbilitiesOperation = "character-creation.allocate-abilities";
    public const string AllocateBackgroundsOperation = "character-creation.allocate-backgrounds";
    public const string InitializeResourcesAndRankOperation = "character-creation.initialize-resources-and-rank";
    public const string SelectRagabashRenownOperation = "character-creation.select-ragabash-renown";
    public const string SetIdentityNameOperation = "character-creation.set-identity-name";
    public const string CompleteCharacterOperation = "character-creation.complete-character";
    public const string DefineActionTestOperation = "character-runtime.define-action-test";
    public const string InterpretActionRollOperation = "character-runtime.interpret-action-roll";
    public const string DefineExtendedTestOperation = "character-runtime.define-extended-test";
    public const string AdvanceExtendedTestOperation = "character-runtime.advance-extended-test";
    public const string DefineResistedTestOperation = "character-runtime.define-resisted-test";
    public const string InterpretResistedTestOperation = "character-runtime.interpret-resisted-test";
    public const string SpendResourceOperation = "character-runtime.spend-resource";
    public const string RecoverResourceOperation = "character-runtime.recover-resource";
    public const string AwardTemporaryRenownOperation = "character-runtime.award-temporary-renown";
    public const string LoseTemporaryRenownOperation = "character-runtime.lose-temporary-renown";
    public const string ConvertTemporaryToPermanentRenownOperation = "character-runtime.convert-temporary-to-permanent-renown";
    public const string ApplyDamageOperation = "character-runtime.apply-damage";
    public const string RecoverDamageOperation = "character-runtime.recover-damage";
    public const string PermanecerAtivoOperation = "character-runtime.permanecer-ativo";
    public const string RegenerateOperation = "character-runtime.regenerate";
    public const string FrenzyDefineTestOperation = "frenzy.define-test";
    public const string FrenzyEnterOperation = "frenzy.enter";
    public const string FrenzySuppressOperation = "frenzy.suppress";
    public const string FrenzyEndOperation = "frenzy.end";
    public const string FrenzyEvaluateActionOperation = "frenzy.evaluate-action";
    public const string DefineInitiativeOperation = "combat.define-initiative";
    public const string DefineAttackOperation = "combat.define-attack";
    public const string DefineDefenseOperation = "combat.define-defense";
    public const string CalculateDamageOperation = "combat.calculate-damage";
    public const string CalculateSoakOperation = "combat.calculate-soak";
    public const string ApplySilverOperation = "combat.apply-silver";
    public const string ApplyRageOperation = "combat.apply-rage";
    public const string ApplyCombatConditionOperation = "combat.apply-combat-condition";
    public const string TransitionCombatStateOperation = "combat.transition-combat-state";
    public const string DefineManeuverOperation = "combat.define-maneuver";
    public const string ResolveActionResolutionOperation = "action-resolution.resolve-action-test";
    public const string ApplyConditionOperation = "action-resolution.apply-condition";
    public const string ClearConditionOperation = "action-resolution.clear-condition";
    public const string EvaluateActionAvailabilityOperation = "action-resolution.evaluate-action-availability";
    public const string PurchaseAdditionalGiftOperation = "character-creation.purchase-additional-gift";
    public const string ExecuteGiftEffectOperation = "gift-runtime.execute-gift-effect";
    public const string ActivateGiftOperation = "gift-runtime.activate-gift";
    public const string CalculateAdvancementCostOperation = "character-runtime.calculate-advancement-cost";
    public const string AdvanceTraitOperation = "character-runtime.advance-trait";
    public const string EvaluateSpecialtyEligibilityOperation = "character-runtime.evaluate-specialty-eligibility";
    public const string EvaluateGiftAdvancementOperation = "character-runtime.evaluate-gift-advancement";
    public const string ExecuteRiteOperation = "rite-runtime.execute-rite";
    public const string InitializeSpiritOperation = WerewolfSpiritMechanicServices.InitializeSpiritOperation;
    public const string EvaluateCrossingOperation = WerewolfSpiritMechanicServices.EvaluateCrossingOperation;
    public const string ComputeMovementSpeedOperation = WerewolfSpiritMechanicServices.ComputeMovementSpeedOperation;
    public const string EvaluateDetectionOperation = WerewolfSpiritMechanicServices.EvaluateDetectionOperation;
    public const string EvaluateMaterializationOperation = WerewolfSpiritMechanicServices.EvaluateMaterializationOperation;
    public const string SpendEssenceOperation = WerewolfSpiritMechanicServices.SpendEssenceOperation;
    public const string ExecuteCharmOperation = WerewolfSpiritMechanicServices.ExecuteCharmOperation;
    public const string EvaluateCommandOperation = WerewolfSpiritMechanicServices.EvaluateCommandOperation;
    public const string EvaluatePossessionOperation = WerewolfSpiritMechanicServices.EvaluatePossessionOperation;
    public const string ApplySpiritDamageOperation = WerewolfSpiritMechanicServices.ApplySpiritDamageOperation;
    public const string SpiritLocationOperation = WerewolfSpiritMechanicServices.SpiritLocationOperation;
    public const string GauntletLookupOperation = WerewolfSpiritMechanicServices.GauntletLookupOperation;
    public const string RealmTravelOperation = WerewolfSpiritMechanicServices.RealmTravelOperation;
    public const string ScenePresenceOperation = WerewolfSpiritMechanicServices.ScenePresenceOperation;
    public const string CaernPelículaOperation = WerewolfSpiritMechanicServices.CaernPelículaOperation;
    public const string PackTotemLinkOperation = WerewolfSpiritMechanicServices.PackTotemLinkOperation;
    public const string SharedTotemEffectsOperation = WerewolfSpiritMechanicServices.SharedTotemEffectsOperation;

    private readonly WerewolfCharacterCreationDraftInitializer characterCreation;
    public WerewolfReferenceRuntime()
        : this(new InMemoryWerewolfCharacterDraftIdentitySource())
    {
    }

    public WerewolfReferenceRuntime(IWerewolfCharacterDraftIdentitySource identitySource)
    {
        characterCreation = new WerewolfCharacterCreationDraftInitializer(identitySource);
    }

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null
    };

    public RuleSetRuntimeMetadata Metadata { get; } = new(
        new RuleSetRuntimeIdentity(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            "Chronicle Werewolf Reference Runtime",
            1),
        WerewolfRuleSetPackage.DeclaredReleaseScope,
        [
            new RuleSetOperationDescriptor(CreateCharacterOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectAuspiceOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectAuspiceGiftOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(AllocateAbilitiesOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(AllocateAttributesOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(AllocateBackgroundsOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(InitializeResourcesAndRankOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SetIdentityNameOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(CompleteCharacterOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectAbilityPrioritiesOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectAttributePrioritiesOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectMetisDeformityOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectRaceOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectRaceGiftOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectRagabashRenownOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectTribeOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectTribeGiftOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(DefineActionTestOperation, "generic-dice", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(InterpretActionRollOperation, "generic-dice", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(DefineExtendedTestOperation, "generic-dice", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(AdvanceExtendedTestOperation, "generic-dice", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(DefineResistedTestOperation, "generic-dice", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(InterpretResistedTestOperation, "generic-dice", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SpendResourceOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(RecoverResourceOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(AwardTemporaryRenownOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(LoseTemporaryRenownOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(ConvertTemporaryToPermanentRenownOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(RecoverDamageOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(ApplyDamageOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(PermanecerAtivoOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(RegenerateOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(FrenzyDefineTestOperation, "frenzy", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(FrenzyEnterOperation, "frenzy", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(FrenzySuppressOperation, "frenzy", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(FrenzyEndOperation, "frenzy", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(FrenzyEvaluateActionOperation, "frenzy", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(DefineInitiativeOperation, "combat", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(DefineAttackOperation, "combat", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(DefineDefenseOperation, "combat", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(CalculateDamageOperation, "combat", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(CalculateSoakOperation, "combat", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(ApplySilverOperation, "combat", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(ApplyRageOperation, "combat", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(ApplyCombatConditionOperation, "combat", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(TransitionCombatStateOperation, "combat", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(DefineManeuverOperation, "combat", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(ResolveActionResolutionOperation, "action-resolution", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(ApplyConditionOperation, "action-resolution", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(ClearConditionOperation, "action-resolution", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(EvaluateActionAvailabilityOperation, "action-resolution", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(PurchaseAdditionalGiftOperation, "additional-gift-purchase", RuleSetOperationStatus.Disabled),
            new RuleSetOperationDescriptor(ActivateGiftOperation, "runtime-gift-activation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(ExecuteGiftEffectOperation, "runtime-gift-execution", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(CalculateAdvancementCostOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(AdvanceTraitOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(EvaluateSpecialtyEligibilityOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(EvaluateGiftAdvancementOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(ExecuteRiteOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(InitializeSpiritOperation, "spirit-umbra", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(EvaluateCrossingOperation, "spirit-umbra", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(ComputeMovementSpeedOperation, "spirit-umbra", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(EvaluateDetectionOperation, "spirit-umbra", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(EvaluateMaterializationOperation, "spirit-umbra", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SpendEssenceOperation, "spirit-umbra", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(ExecuteCharmOperation, "spirit-umbra", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(EvaluateCommandOperation, "spirit-umbra", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(EvaluatePossessionOperation, "spirit-umbra", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(ApplySpiritDamageOperation, "spirit-umbra", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(WerewolfSpiritMechanicServices.SpiritLocationOperation, "spirit-umbra", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(WerewolfSpiritMechanicServices.GauntletLookupOperation, "spirit-umbra", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(WerewolfSpiritMechanicServices.RealmTravelOperation, "spirit-umbra", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(WerewolfSpiritMechanicServices.ScenePresenceOperation, "spirit-umbra", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(WerewolfSpiritMechanicServices.CaernPelículaOperation, "spirit-umbra", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(WerewolfSpiritMechanicServices.PackTotemLinkOperation, "spirit-umbra", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(WerewolfSpiritMechanicServices.SharedTotemEffectsOperation, "spirit-umbra", RuleSetOperationStatus.Enabled)
        ]);

    public RuleSetOperationResult Execute(RuleSetOperationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (StringComparer.Ordinal.Equals(request.OperationKey, SelectRaceOperation))
        {
            return ExecuteSelectRace(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, SelectAuspiceOperation))
        {
            return ExecuteSelectAuspice(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, SelectTribeOperation))
        {
            return ExecuteSelectTribe(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, SelectMetisDeformityOperation))
        {
            return ExecuteSelectMetisDeformity(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, SelectRaceGiftOperation))
        {
            return ExecuteSelectInitialGift(request, WerewolfInitialGiftSource.Race);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, SelectAuspiceGiftOperation))
        {
            return ExecuteSelectInitialGift(request, WerewolfInitialGiftSource.Auspice);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, SelectTribeGiftOperation))
        {
            return ExecuteSelectInitialGift(request, WerewolfInitialGiftSource.Tribe);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, SelectAttributePrioritiesOperation))
        {
            return ExecuteSelectAttributePriorities(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, AllocateAttributesOperation))
        {
            return ExecuteAllocateAttributes(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, SelectAbilityPrioritiesOperation))
        {
            return ExecuteSelectAbilityPriorities(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, AllocateAbilitiesOperation))
        {
            return ExecuteAllocateAbilities(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, AllocateBackgroundsOperation))
        {
            return ExecuteAllocateBackgrounds(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, InitializeResourcesAndRankOperation))
        {
            return ExecuteInitializeResourcesAndRank(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, SelectRagabashRenownOperation))
        {
            return ExecuteSelectRagabashRenown(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, SetIdentityNameOperation))
        {
            return ExecuteSetIdentityName(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, CompleteCharacterOperation))
        {
            return ExecuteCompleteCharacter(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, DefineActionTestOperation))
        {
            return ExecuteDefineActionTest(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, InterpretActionRollOperation))
        {
            return ExecuteInterpretActionRoll(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, DefineExtendedTestOperation))
        {
            return ExecuteDefineExtendedTest(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, AdvanceExtendedTestOperation))
        {
            return ExecuteAdvanceExtendedTest(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, DefineResistedTestOperation))
        {
            return ExecuteDefineResistedTest(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, InterpretResistedTestOperation))
        {
            return ExecuteInterpretResistedTest(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, SpendResourceOperation))
        {
            return ExecuteSpendResource(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, RecoverResourceOperation))
        {
            return ExecuteRecoverResource(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, AwardTemporaryRenownOperation))
        {
            return ExecuteAwardTemporaryRenown(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, LoseTemporaryRenownOperation))
        {
            return ExecuteLoseTemporaryRenown(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, ConvertTemporaryToPermanentRenownOperation))
        {
            return ExecuteConvertTemporaryToPermanentRenown(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, ApplyDamageOperation))
        {
            return ExecuteApplyDamage(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, RecoverDamageOperation))
        {
            return ExecuteRecoverDamage(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, PermanecerAtivoOperation))
        {
            return ExecutePermanecerAtivo(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, RegenerateOperation))
        {
            return ExecuteRegenerate(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, FrenzyDefineTestOperation))
        {
            return ExecuteFrenzyDefineTest(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, FrenzyEnterOperation))
        {
            return ExecuteFrenzyEnter(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, FrenzySuppressOperation))
        {
            return ExecuteFrenzySuppress(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, FrenzyEndOperation))
        {
            return ExecuteFrenzyEnd(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, FrenzyEvaluateActionOperation))
        {
            return ExecuteFrenzyEvaluateAction(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, DefineInitiativeOperation))
        {
            return ExecuteDefineInitiative(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, DefineAttackOperation))
        {
            return ExecuteDefineAttack(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, DefineDefenseOperation))
        {
            return ExecuteDefineDefense(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, CalculateDamageOperation))
        {
            return ExecuteCalculateDamage(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, CalculateSoakOperation))
        {
            return ExecuteCalculateSoak(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, ApplySilverOperation))
        {
            return ExecuteApplySilver(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, ApplyRageOperation))
        {
            return ExecuteApplyRage(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, ApplyCombatConditionOperation))
        {
            return ExecuteApplyCombatCondition(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, TransitionCombatStateOperation))
        {
            return ExecuteTransitionCombatState(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, DefineManeuverOperation))
        {
            return ExecuteDefineManeuver(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, ResolveActionResolutionOperation))
        {
            return ExecuteResolveActionResolution(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, ApplyConditionOperation))
        {
            return ExecuteApplyCondition(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, ClearConditionOperation))
        {
            return ExecuteClearCondition(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, EvaluateActionAvailabilityOperation))
        {
            return ExecuteEvaluateActionAvailability(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, ActivateGiftOperation))
        {
            return ExecuteActivateGift(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, ExecuteGiftEffectOperation))
        {
            return ExecuteGiftEffect(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, CalculateAdvancementCostOperation))
        {
            return ExecuteCalculateAdvancementCost(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, AdvanceTraitOperation))
        {
            return ExecuteAdvanceTrait(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, EvaluateSpecialtyEligibilityOperation))
        {
            return ExecuteEvaluateSpecialtyEligibility(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, EvaluateGiftAdvancementOperation))
        {
            return ExecuteEvaluateGiftAdvancement(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, ExecuteRiteOperation))
        {
            return ExecuteRite(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, InitializeSpiritOperation))
        {
            return ExecuteInitializeSpirit(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, EvaluateCrossingOperation))
        {
            return ExecuteEvaluateCrossing(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, ComputeMovementSpeedOperation))
        {
            return ExecuteComputeMovementSpeed(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, EvaluateDetectionOperation))
        {
            return ExecuteEvaluateDetection(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, EvaluateMaterializationOperation))
        {
            return ExecuteEvaluateMaterialization(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, SpendEssenceOperation))
        {
            return ExecuteSpendEssence(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, ExecuteCharmOperation))
        {
            return ExecuteCharm(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, EvaluateCommandOperation))
        {
            return ExecuteEvaluateCommand(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, EvaluatePossessionOperation))
        {
            return ExecuteEvaluatePossession(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, ApplySpiritDamageOperation))
        {
            return ExecuteApplySpiritDamage(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, WerewolfSpiritMechanicServices.SpiritLocationOperation))
        {
            return ExecuteSpiritLocation(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, WerewolfSpiritMechanicServices.GauntletLookupOperation))
        {
            return ExecuteGauntletLookup(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, WerewolfSpiritMechanicServices.RealmTravelOperation))
        {
            return ExecuteRealmTravel(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, WerewolfSpiritMechanicServices.ScenePresenceOperation))
        {
            return ExecuteScenePresence(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, WerewolfSpiritMechanicServices.CaernPelículaOperation))
        {
            return ExecuteCaernPelícula(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, WerewolfSpiritMechanicServices.PackTotemLinkOperation))
        {
            return ExecutePackTotemLink(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, WerewolfSpiritMechanicServices.SharedTotemEffectsOperation))
        {
            return ExecuteSharedTotemEffects(request);
        }

        if (!StringComparer.Ordinal.Equals(request.OperationKey, CreateCharacterOperation))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.OperationUndeclared,
            [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "OperationUndeclared", "Werewolf reference runtime does not implement the requested operation.")],
            new Dictionary<string, string>(StringComparer.Ordinal));
        }

        if (!request.Inputs.TryGetValue("requestId", out var requestId) || string.IsNullOrWhiteSpace(requestId))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "MissingRequestId", "Create-character request requires a deterministic request id.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var payload = characterCreation.Initialize(new WerewolfCreateCharacterRequest(requestId));
        if (!payload.Succeeded || payload.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                payload.Findings.Select(finding => new RuleSetRuntimeFinding(
                        finding.Severity == WerewolfCharacterInitializationFindingSeverity.Error
                            ? RuleSetRuntimeFindingSeverity.Error
                            : RuleSetRuntimeFindingSeverity.Information,
                        finding.Code,
                        finding.Message))
                    .ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            payload.Findings.Select(finding => new RuleSetRuntimeFinding(
                    RuleSetRuntimeFindingSeverity.Information,
                    finding.Code,
                    finding.Message))
                .ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["draftId"] = payload.Draft.DraftIdentity.Value,
                ["draftStatus"] = payload.Draft.Status.ToString(),
                ["draftVersion"] = payload.Draft.DraftVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["attributePriorityOrder"] = string.Join(",", payload.Draft.AttributePriorityOrder),
                ["attributeBudgets"] = FormatBudgets(payload.Draft.AttributeBudgets),
                ["abilityPriorityOrder"] = string.Join(",", payload.Draft.AbilityPriorityOrder),
                ["abilityBudgets"] = FormatBudgets(payload.Draft.AbilityBudgets),
                ["backgrounds"] = FormatNullableRatings(payload.Draft.Backgrounds),
                ["resources"] = FormatNullableRatings(payload.Draft.Resources),
                ["renown"] = FormatNullableRatings(payload.Draft.Renown),
                ["rankId"] = payload.Draft.Rank ?? string.Empty,
                ["rankValue"] = payload.Draft.RankValue?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["raceGiftId"] = payload.Draft.RaceGift ?? string.Empty,
                ["auspiceGiftId"] = payload.Draft.AuspiceGift ?? string.Empty,
                ["tribeGiftId"] = payload.Draft.TribeGift ?? string.Empty,
                ["nextSteps"] = string.Join(",", payload.Draft.RequiredNextSteps)
            });
    }

    private static RuleSetOperationResult ExecuteSelectRace(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("raceId", out var raceId) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidRaceSelectionRequest", "Race selection requires draftId, draftVersion, expectedDraftVersion, and raceId.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentRace = request.Inputs.GetValueOrDefault("currentRace");
        var currentRaceGift = request.Inputs.GetValueOrDefault("currentRaceGift");
        var currentAuspiceGift = request.Inputs.GetValueOrDefault("currentAuspiceGift");
        var currentTribeGift = request.Inputs.GetValueOrDefault("currentTribeGift");
        var attributePriorityOrder = ParseCsv(request.Inputs.GetValueOrDefault("attributePriorityOrder"));
        var attributeBudgets = ParseBudgets(request.Inputs.GetValueOrDefault("attributeBudgets"));
        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion) with
        {
            Race = string.IsNullOrWhiteSpace(currentRace) ? null : currentRace,
            RaceGift = string.IsNullOrWhiteSpace(currentRaceGift) ? null : currentRaceGift,
            AuspiceGift = string.IsNullOrWhiteSpace(currentAuspiceGift) ? null : currentAuspiceGift,
            TribeGift = string.IsNullOrWhiteSpace(currentTribeGift) ? null : currentTribeGift,
            AttributePriorityOrder = Array.AsReadOnly(attributePriorityOrder),
            AttributeBudgets = attributeBudgets
        };

        var result = WerewolfRaceSelectionService.SelectRace(new WerewolfRaceSelectionRequest(draft, expectedVersion, raceId));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, finding.Code.ToString(), finding.Message)).ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["draftId"] = result.Draft.DraftIdentity.Value,
                ["draftVersion"] = result.Draft.DraftVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["attributePriorityOrder"] = string.Join(",", result.Draft.AttributePriorityOrder),
                ["attributeBudgets"] = FormatBudgets(result.Draft.AttributeBudgets),
                ["metisDeformityId"] = result.Draft.MetisDeformity ?? string.Empty,
                ["raceId"] = result.Draft.Race ?? string.Empty,
                ["raceGiftId"] = result.Draft.RaceGift ?? string.Empty,
                ["auspiceGiftId"] = result.Draft.AuspiceGift ?? string.Empty,
                ["tribeGiftId"] = result.Draft.TribeGift ?? string.Empty,
                ["nextSteps"] = string.Join(",", result.Draft.RequiredNextSteps)
            });
    }

    private static RuleSetOperationResult ExecuteSelectAuspice(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("auspiceId", out var auspiceId) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidAuspiceSelectionRequest", "Auspice selection requires draftId, draftVersion, expectedDraftVersion, and auspiceId.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentRace = request.Inputs.GetValueOrDefault("currentRace");
        var currentAuspice = request.Inputs.GetValueOrDefault("currentAuspice");
        var currentMetisDeformity = request.Inputs.GetValueOrDefault("currentMetisDeformity");
        var currentRaceGift = request.Inputs.GetValueOrDefault("currentRaceGift");
        var currentAuspiceGift = request.Inputs.GetValueOrDefault("currentAuspiceGift");
        var currentTribeGift = request.Inputs.GetValueOrDefault("currentTribeGift");
        var attributePriorityOrder = ParseCsv(request.Inputs.GetValueOrDefault("attributePriorityOrder"));
        var attributeBudgets = ParseBudgets(request.Inputs.GetValueOrDefault("attributeBudgets"));
        var metisRequirement = request.Inputs.TryGetValue("requiresMetisDeformity", out var requiresMetisDeformity) &&
            StringComparer.Ordinal.Equals(requiresMetisDeformity, "true");
        var nextSteps = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion).RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-race"))
            .ToList();

        if (metisRequirement && !nextSteps.Contains("select-metis-deformity", StringComparer.Ordinal))
        {
            nextSteps.Add("select-metis-deformity");
        }

        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion) with
        {
            Race = string.IsNullOrWhiteSpace(currentRace) ? null : currentRace,
            Auspice = string.IsNullOrWhiteSpace(currentAuspice) ? null : currentAuspice,
            MetisDeformity = string.IsNullOrWhiteSpace(currentMetisDeformity) ? null : currentMetisDeformity,
            RaceGift = string.IsNullOrWhiteSpace(currentRaceGift) ? null : currentRaceGift,
            AuspiceGift = string.IsNullOrWhiteSpace(currentAuspiceGift) ? null : currentAuspiceGift,
            TribeGift = string.IsNullOrWhiteSpace(currentTribeGift) ? null : currentTribeGift,
            AttributePriorityOrder = Array.AsReadOnly(attributePriorityOrder),
            AttributeBudgets = attributeBudgets,
            RequiredNextSteps = Array.AsReadOnly(nextSteps.Order(StringComparer.Ordinal).ToArray())
        };

        var result = WerewolfAuspiceSelectionService.SelectAuspice(new WerewolfAuspiceSelectionRequest(draft, expectedVersion, auspiceId));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, finding.Code.ToString(), finding.Message)).ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["auspiceId"] = result.Draft.Auspice ?? string.Empty,
                ["draftId"] = result.Draft.DraftIdentity.Value,
                ["draftVersion"] = result.Draft.DraftVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["attributePriorityOrder"] = string.Join(",", result.Draft.AttributePriorityOrder),
                ["attributeBudgets"] = FormatBudgets(result.Draft.AttributeBudgets),
                ["metisDeformityId"] = result.Draft.MetisDeformity ?? string.Empty,
                ["nextSteps"] = string.Join(",", result.Draft.RequiredNextSteps),
                ["raceId"] = result.Draft.Race ?? string.Empty,
                ["raceGiftId"] = result.Draft.RaceGift ?? string.Empty,
                ["auspiceGiftId"] = result.Draft.AuspiceGift ?? string.Empty,
                ["tribeGiftId"] = result.Draft.TribeGift ?? string.Empty
            });
    }

    private static RuleSetOperationResult ExecuteSelectTribe(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("tribeId", out var tribeId) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidTribeSelectionRequest", "Tribe selection requires draftId, draftVersion, expectedDraftVersion, and tribeId.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentRace = request.Inputs.GetValueOrDefault("currentRace");
        var currentAuspice = request.Inputs.GetValueOrDefault("currentAuspice");
        var currentTribe = request.Inputs.GetValueOrDefault("currentTribe");
        var currentMetisDeformity = request.Inputs.GetValueOrDefault("currentMetisDeformity");
        var currentRaceGift = request.Inputs.GetValueOrDefault("currentRaceGift");
        var currentAuspiceGift = request.Inputs.GetValueOrDefault("currentAuspiceGift");
        var currentTribeGift = request.Inputs.GetValueOrDefault("currentTribeGift");
        var attributePriorityOrder = ParseCsv(request.Inputs.GetValueOrDefault("attributePriorityOrder"));
        var attributeBudgets = ParseBudgets(request.Inputs.GetValueOrDefault("attributeBudgets"));
        var metisRequirement = request.Inputs.TryGetValue("requiresMetisDeformity", out var requiresMetisDeformity) &&
            StringComparer.Ordinal.Equals(requiresMetisDeformity, "true");
        var defaultNextSteps = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion).RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-race"))
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-auspice"))
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-tribe"))
            .ToList();

        var nextSteps = request.Inputs.TryGetValue("nextSteps", out var nextStepsText) && !string.IsNullOrWhiteSpace(nextStepsText)
            ? nextStepsText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            : defaultNextSteps;

        if (metisRequirement && !nextSteps.Contains("select-metis-deformity", StringComparer.Ordinal))
        {
            nextSteps.Add("select-metis-deformity");
        }

        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion) with
        {
            Race = string.IsNullOrWhiteSpace(currentRace) ? null : currentRace,
            Auspice = string.IsNullOrWhiteSpace(currentAuspice) ? null : currentAuspice,
            Tribe = string.IsNullOrWhiteSpace(currentTribe) ? null : currentTribe,
            MetisDeformity = string.IsNullOrWhiteSpace(currentMetisDeformity) ? null : currentMetisDeformity,
            RaceGift = string.IsNullOrWhiteSpace(currentRaceGift) ? null : currentRaceGift,
            AuspiceGift = string.IsNullOrWhiteSpace(currentAuspiceGift) ? null : currentAuspiceGift,
            TribeGift = string.IsNullOrWhiteSpace(currentTribeGift) ? null : currentTribeGift,
            AttributePriorityOrder = Array.AsReadOnly(attributePriorityOrder),
            AttributeBudgets = attributeBudgets,
            RequiredNextSteps = Array.AsReadOnly(nextSteps.Order(StringComparer.Ordinal).ToArray())
        };

        var result = WerewolfTribeSelectionService.SelectTribe(new WerewolfTribeSelectionRequest(draft, expectedVersion, tribeId));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, finding.Code.ToString(), finding.Message)).ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["auspiceId"] = result.Draft.Auspice ?? string.Empty,
                ["draftId"] = result.Draft.DraftIdentity.Value,
                ["draftVersion"] = result.Draft.DraftVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["attributePriorityOrder"] = string.Join(",", result.Draft.AttributePriorityOrder),
                ["attributeBudgets"] = FormatBudgets(result.Draft.AttributeBudgets),
                ["metisDeformityId"] = result.Draft.MetisDeformity ?? string.Empty,
                ["nextSteps"] = string.Join(",", result.Draft.RequiredNextSteps),
                ["raceId"] = result.Draft.Race ?? string.Empty,
                ["tribeId"] = result.Draft.Tribe ?? string.Empty,
                ["raceGiftId"] = result.Draft.RaceGift ?? string.Empty,
                ["auspiceGiftId"] = result.Draft.AuspiceGift ?? string.Empty,
                ["tribeGiftId"] = result.Draft.TribeGift ?? string.Empty
            });
    }

    private static RuleSetOperationResult ExecuteSelectMetisDeformity(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("deformityId", out var deformityId) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidMetisDeformitySelectionRequest", "Metis deformity selection requires draftId, draftVersion, expectedDraftVersion, and deformityId.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentRace = request.Inputs.GetValueOrDefault("currentRace");
        var currentAuspice = request.Inputs.GetValueOrDefault("currentAuspice");
        var currentTribe = request.Inputs.GetValueOrDefault("currentTribe");
        var currentMetisDeformity = request.Inputs.GetValueOrDefault("currentMetisDeformity");
        var currentRaceGift = request.Inputs.GetValueOrDefault("currentRaceGift");
        var currentAuspiceGift = request.Inputs.GetValueOrDefault("currentAuspiceGift");
        var currentTribeGift = request.Inputs.GetValueOrDefault("currentTribeGift");
        var attributePriorityOrder = ParseCsv(request.Inputs.GetValueOrDefault("attributePriorityOrder"));
        var attributeBudgets = ParseBudgets(request.Inputs.GetValueOrDefault("attributeBudgets"));
        var defaultNextSteps = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion).RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-race"))
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-auspice"))
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-tribe"))
            .ToList();

        var nextSteps = request.Inputs.TryGetValue("nextSteps", out var nextStepsText) && !string.IsNullOrWhiteSpace(nextStepsText)
            ? nextStepsText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            : defaultNextSteps;

        if (attributePriorityOrder.Length > 0 || attributeBudgets.Count > 0)
        {
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "select-attribute-priorities"));
        }

        if (!nextSteps.Contains("select-metis-deformity", StringComparer.Ordinal))
        {
            nextSteps.Add("select-metis-deformity");
        }

        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion) with
        {
            Race = string.IsNullOrWhiteSpace(currentRace) ? null : currentRace,
            Auspice = string.IsNullOrWhiteSpace(currentAuspice) ? null : currentAuspice,
            Tribe = string.IsNullOrWhiteSpace(currentTribe) ? null : currentTribe,
            MetisDeformity = string.IsNullOrWhiteSpace(currentMetisDeformity) ? null : currentMetisDeformity,
            RaceGift = string.IsNullOrWhiteSpace(currentRaceGift) ? null : currentRaceGift,
            AuspiceGift = string.IsNullOrWhiteSpace(currentAuspiceGift) ? null : currentAuspiceGift,
            TribeGift = string.IsNullOrWhiteSpace(currentTribeGift) ? null : currentTribeGift,
            AttributePriorityOrder = Array.AsReadOnly(attributePriorityOrder),
            AttributeBudgets = attributeBudgets,
            RequiredNextSteps = Array.AsReadOnly(nextSteps.Order(StringComparer.Ordinal).ToArray())
        };

        var result = WerewolfMetisDeformitySelectionService.SelectDeformity(new WerewolfMetisDeformitySelectionRequest(draft, expectedVersion, deformityId));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, finding.Code.ToString(), finding.Message)).ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["auspiceId"] = result.Draft.Auspice ?? string.Empty,
                ["draftId"] = result.Draft.DraftIdentity.Value,
                ["draftVersion"] = result.Draft.DraftVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["attributePriorityOrder"] = string.Join(",", result.Draft.AttributePriorityOrder),
                ["attributeBudgets"] = FormatBudgets(result.Draft.AttributeBudgets),
                ["metisDeformityId"] = result.Draft.MetisDeformity ?? string.Empty,
                ["nextSteps"] = string.Join(",", result.Draft.RequiredNextSteps),
                ["raceId"] = result.Draft.Race ?? string.Empty,
                ["tribeId"] = result.Draft.Tribe ?? string.Empty,
                ["raceGiftId"] = result.Draft.RaceGift ?? string.Empty,
                ["auspiceGiftId"] = result.Draft.AuspiceGift ?? string.Empty,
                ["tribeGiftId"] = result.Draft.TribeGift ?? string.Empty
            });
    }

    private static RuleSetOperationResult ExecuteSelectInitialGift(RuleSetOperationRequest request, WerewolfInitialGiftSource source)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("giftId", out var giftId) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidInitialGiftSelectionRequest", "Initial Gift selection requires draftId, draftVersion, expectedDraftVersion, and giftId.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var draft = BuildDraftFromInputs(request, draftId, draftVersion);
        var result = WerewolfInitialGiftSelectionService.SelectGift(new WerewolfInitialGiftSelectionRequest(draft, expectedVersion, source, giftId));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return ToDraftOperationResult(
            result.Draft,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, finding.Code.ToString(), finding.Message)).ToArray());
    }

    private static RuleSetOperationResult ExecuteSelectAttributePriorities(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("primaryCategoryId", out var primaryCategoryId) ||
            !request.Inputs.TryGetValue("secondaryCategoryId", out var secondaryCategoryId) ||
            !request.Inputs.TryGetValue("tertiaryCategoryId", out var tertiaryCategoryId) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidAttributePrioritySelectionRequest", "Attribute priority selection requires draftId, draftVersion, expectedDraftVersion, primaryCategoryId, secondaryCategoryId, and tertiaryCategoryId.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var draft = BuildDraftFromInputs(request, draftId, draftVersion);
        var result = WerewolfAttributePrioritySelectionService.SelectPriorities(new WerewolfAttributePrioritySelectionRequest(
            draft,
            expectedVersion,
            primaryCategoryId,
            secondaryCategoryId,
            tertiaryCategoryId));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return ToDraftOperationResult(
            result.Draft,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, finding.Code.ToString(), finding.Message)).ToArray());
    }

    private static RuleSetOperationResult ExecuteAllocateAttributes(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("attributes", out var attributesText) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidAttributeAllocationRequest", "Attribute allocation requires draftId, draftVersion, expectedDraftVersion, and attributes.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var draft = BuildDraftFromInputs(request, draftId, draftVersion);
        var allocations = ParseAttributeAllocations(attributesText);
        var result = WerewolfAttributeAllocationService.AllocateAttributes(new WerewolfAttributeAllocationRequest(draft, expectedVersion, allocations));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["attributeCategoryTotals"] = FormatAttributeCategoryTotals(result.CategoryTotals)
                });
        }

        return ToDraftOperationResult(
            result.Draft,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, finding.Code.ToString(), finding.Message)).ToArray(),
            result.CategoryTotals);
    }

    private static RuleSetOperationResult ExecuteSelectAbilityPriorities(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("primaryCategoryId", out var primaryCategoryId) ||
            !request.Inputs.TryGetValue("secondaryCategoryId", out var secondaryCategoryId) ||
            !request.Inputs.TryGetValue("tertiaryCategoryId", out var tertiaryCategoryId) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidAbilityPrioritySelectionRequest", "Ability priority selection requires draftId, draftVersion, expectedDraftVersion, primaryCategoryId, secondaryCategoryId, and tertiaryCategoryId.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var draft = BuildDraftFromInputs(request, draftId, draftVersion);
        var result = WerewolfAbilitySelectionService.SelectPriorities(new WerewolfAbilityPrioritySelectionRequest(
            draft,
            expectedVersion,
            primaryCategoryId,
            secondaryCategoryId,
            tertiaryCategoryId));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return ToDraftOperationResult(
            result.Draft,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, finding.Code.ToString(), finding.Message)).ToArray());
    }

    private static RuleSetOperationResult ExecuteAllocateAbilities(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("abilities", out var abilitiesText) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidAbilityAllocationRequest", "Ability allocation requires draftId, draftVersion, expectedDraftVersion, and abilities.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var draft = BuildDraftFromInputs(request, draftId, draftVersion);
        var result = WerewolfAbilitySelectionService.AllocateAbilities(new WerewolfAbilityAllocationRequest(draft, expectedVersion, ParseAbilityAllocations(abilitiesText)));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["abilityCategoryTotals"] = FormatAbilityCategoryTotals(result.CategoryTotals)
                });
        }

        return ToDraftOperationResult(
            result.Draft,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, finding.Code.ToString(), finding.Message)).ToArray(),
            null,
            result.CategoryTotals);
    }

    private static RuleSetOperationResult ExecuteAllocateBackgrounds(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("backgrounds", out var backgroundsText) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidBackgroundAllocationRequest", "Background allocation requires draftId, draftVersion, expectedDraftVersion, and backgrounds.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var draft = BuildDraftFromInputs(request, draftId, draftVersion);
        var result = WerewolfBackgroundAllocationService.AllocateBackgrounds(new WerewolfBackgroundAllocationRequest(draft, expectedVersion, ParseBackgroundAllocations(backgroundsText)));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["backgroundTotal"] = FormatBackgroundTotal(result.Spent, result.Budget, result.Remaining)
                });
        }

        return ToDraftOperationResult(
            result.Draft,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, finding.Code.ToString(), finding.Message)).ToArray(),
            null,
            null,
            result);
    }

    private static RuleSetOperationResult ExecuteInitializeResourcesAndRank(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidResourceRankInitializationRequest", "Resource and Rank initialization requires draftId, draftVersion, and expectedDraftVersion.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var draft = BuildDraftFromInputs(request, draftId, draftVersion);
        var result = WerewolfResourceRankInitializationService.Initialize(new WerewolfResourceRankInitializationRequest(draft, expectedVersion));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return ToDraftOperationResult(
            result.Draft,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(
                    finding.Severity == WerewolfResourceRankInitializationFindingSeverity.Error ? RuleSetRuntimeFindingSeverity.Error : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code.ToString(),
                    finding.Message))
                .ToArray());
    }

    private static RuleSetOperationResult ExecuteSelectRagabashRenown(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("glory", out var gloryText) ||
            !request.Inputs.TryGetValue("honor", out var honorText) ||
            !request.Inputs.TryGetValue("wisdom", out var wisdomText) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
            !int.TryParse(gloryText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var glory) ||
            !int.TryParse(honorText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var honor) ||
            !int.TryParse(wisdomText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var wisdom))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidRagabashRenownRequest", "Ragabash Renown selection requires draftId, draftVersion, expectedDraftVersion, glory, honor, and wisdom.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var draft = BuildDraftFromInputs(request, draftId, draftVersion);
        var result = WerewolfRagabashRenownSelectionService.SelectRenown(new WerewolfRagabashRenownSelectionRequest(draft, expectedVersion, glory, honor, wisdom));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return ToDraftOperationResult(
            result.Draft,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(
                    finding.Severity == WerewolfRagabashRenownSelectionFindingSeverity.Error ? RuleSetRuntimeFindingSeverity.Error : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code.ToString(),
                    finding.Message))
                .ToArray());
    }

    private static RuleSetOperationResult ExecuteSetIdentityName(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("identityName", out var identityName) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidIdentityNameRequest", "Identity name operation requires draftId, draftVersion, expectedDraftVersion, and identityName.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var draft = BuildDraftFromInputs(request, draftId, draftVersion);
        var result = WerewolfIdentityNameOperation.SetIdentityName(new WerewolfIdentityNameRequest(draft, expectedVersion, identityName));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return ToDraftOperationResult(
            result.Draft,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(
                    finding.Severity == WerewolfIdentityNameFindingSeverity.Error ? RuleSetRuntimeFindingSeverity.Error : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code.ToString(),
                    finding.Message))
                .ToArray());
    }

    private static RuleSetOperationResult ExecuteCompleteCharacter(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidCompletionRequest", "Completion operation requires draftId, draftVersion, and expectedDraftVersion.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var draft = BuildDraftFromInputs(request, draftId, draftVersion);
        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, expectedVersion));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var snapshotJson = System.Text.Json.JsonSerializer.Serialize(result.Snapshot, JsonOptions);
        var runtimeState = WerewolfRuntimeCharacterState.FromSnapshot(result.Snapshot!);
        var newStateJson = System.Text.Json.JsonSerializer.Serialize(runtimeState, JsonOptions);

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(
                    finding.Severity == WerewolfCharacterCompletionFindingSeverity.Error ? RuleSetRuntimeFindingSeverity.Error : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code.ToString(),
                    finding.Message))
                .ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["draftId"] = result.Draft.DraftIdentity.Value,
                ["draftVersion"] = result.Draft.DraftVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["status"] = result.Draft.Status.ToString(),
                ["identityName"] = result.Draft.IdentityName ?? string.Empty,
                ["nextSteps"] = string.Join(",", result.Draft.RequiredNextSteps),
                ["validationFingerprint"] = result.Snapshot!.ValidationFingerprint,
                ["completedStepKeys"] = string.Join(",", result.Snapshot.CompletedStepKeys),
                ["snapshot"] = snapshotJson,
                ["newState"] = newStateJson
            });
    }

    private static WerewolfInitializedCharacterState BuildDraftFromInputs(RuleSetOperationRequest request, string draftId, int draftVersion)
    {
        var currentRace = request.Inputs.GetValueOrDefault("currentRace");
        var currentAuspice = request.Inputs.GetValueOrDefault("currentAuspice");
        var currentTribe = request.Inputs.GetValueOrDefault("currentTribe");
        var currentMetisDeformity = request.Inputs.GetValueOrDefault("currentMetisDeformity");
        var currentRaceGift = request.Inputs.GetValueOrDefault("currentRaceGift");
        var currentAuspiceGift = request.Inputs.GetValueOrDefault("currentAuspiceGift");
        var currentTribeGift = request.Inputs.GetValueOrDefault("currentTribeGift");
        var attributePriorityOrder = ParseCsv(request.Inputs.GetValueOrDefault("attributePriorityOrder"));
        var attributeBudgets = ParseBudgets(request.Inputs.GetValueOrDefault("attributeBudgets"));
        var attributes = ParseNullableAttributes(request.Inputs.GetValueOrDefault("attributes"));
        var abilityPriorityOrder = ParseCsv(request.Inputs.GetValueOrDefault("abilityPriorityOrder"));
        var abilityBudgets = ParseBudgets(request.Inputs.GetValueOrDefault("abilityBudgets"));
        var abilities = ParseNullableRatings(request.Inputs.GetValueOrDefault("abilities"));
        var backgrounds = ParseNullableRatings(request.Inputs.GetValueOrDefault("backgrounds"));
        var resources = ParseNullableRatings(request.Inputs.GetValueOrDefault("resources"));
        var renown = ParseNullableRatings(request.Inputs.GetValueOrDefault("renown"));
        var rank = request.Inputs.GetValueOrDefault("rankId");
        var rankValue = request.Inputs.TryGetValue("rankValue", out var rankValueText) &&
            int.TryParse(rankValueText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var parsedRankValue)
            ? parsedRankValue
            : (int?)null;
        var identityName = request.Inputs.GetValueOrDefault("identityName");
        var draftStatus = request.Inputs.TryGetValue("draftStatus", out var draftStatusText) &&
            Enum.TryParse<WerewolfCharacterDraftStatus>(draftStatusText, true, out var parsedDraftStatus)
            ? parsedDraftStatus
            : WerewolfCharacterDraftStatus.Initialized;

        var defaultNextSteps = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion).RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-race"))
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-auspice"))
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-tribe"))
            .ToList();

        var nextSteps = request.Inputs.TryGetValue("nextSteps", out var nextStepsText) && !string.IsNullOrWhiteSpace(nextStepsText)
            ? nextStepsText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            : defaultNextSteps;

        if (attributePriorityOrder.Length > 0 || attributeBudgets.Count > 0)
        {
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "select-attribute-priorities"));
        }

        if (abilityPriorityOrder.Length > 0 || abilityBudgets.Count > 0)
        {
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "select-ability-priorities"));
        }

        if (attributes.Count > 0)
        {
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "allocate-attributes"));
        }

        if (abilities.Count > 0)
        {
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "allocate-abilities"));
        }

        if (backgrounds.Count > 0)
        {
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "allocate-backgrounds"));
        }

        if (!string.IsNullOrWhiteSpace(currentRaceGift) && !string.IsNullOrWhiteSpace(currentAuspiceGift) && !string.IsNullOrWhiteSpace(currentTribeGift))
        {
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "select-initial-gifts"));
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "select-race-gift"));
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "select-auspice-gift"));
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "select-tribe-gift"));
        }

        if (resources.Count > 0 || !string.IsNullOrWhiteSpace(rank))
        {
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "initialize-resources-and-rank"));
        }

        if (!string.IsNullOrWhiteSpace(request.Inputs.GetValueOrDefault("identityName")))
        {
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "set-identity-name"));
        }

        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion) with
        {
            Status = draftStatus,
            Race = string.IsNullOrWhiteSpace(currentRace) ? null : currentRace,
            Auspice = string.IsNullOrWhiteSpace(currentAuspice) ? null : currentAuspice,
            Tribe = string.IsNullOrWhiteSpace(currentTribe) ? null : currentTribe,
            MetisDeformity = string.IsNullOrWhiteSpace(currentMetisDeformity) ? null : currentMetisDeformity,
            RaceGift = string.IsNullOrWhiteSpace(currentRaceGift) ? null : currentRaceGift,
            AuspiceGift = string.IsNullOrWhiteSpace(currentAuspiceGift) ? null : currentAuspiceGift,
            TribeGift = string.IsNullOrWhiteSpace(currentTribeGift) ? null : currentTribeGift,
            AttributePriorityOrder = Array.AsReadOnly(attributePriorityOrder),
            AttributeBudgets = attributeBudgets,
            AbilityPriorityOrder = Array.AsReadOnly(abilityPriorityOrder),
            AbilityBudgets = abilityBudgets,
            Attributes = attributes.Count == 0
                ? WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion).Attributes
                : attributes,
            Abilities = abilities.Count == 0
                ? WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion).Abilities
                : abilities,
            Backgrounds = backgrounds.Count == 0
                ? WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion).Backgrounds
                : backgrounds,
            Resources = resources.Count == 0
                ? WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion).Resources
                : resources,
            Renown = renown.Count == 0
                ? WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion).Renown
                : renown,
            Rank = string.IsNullOrWhiteSpace(rank) ? null : rank,
            RankValue = rankValue,
            IdentityName = string.IsNullOrWhiteSpace(identityName) ? null : identityName,
            RequiredNextSteps = Array.AsReadOnly(nextSteps.Order(StringComparer.Ordinal).ToArray())
        };

        return draft with
        {
            RequiredNextSteps = WerewolfInitialGiftSelectionService.ReconcileInitialGiftNextSteps(draft)
        };
    }

    private static RuleSetOperationResult ToDraftOperationResult(
        WerewolfInitializedCharacterState draft,
        IReadOnlyList<RuleSetRuntimeFinding> findings,
        IReadOnlyList<WerewolfAttributeAllocationCategoryTotal>? attributeCategoryTotals = null,
        IReadOnlyList<WerewolfAbilityAllocationCategoryTotal>? abilityCategoryTotals = null,
        WerewolfBackgroundAllocationResult? backgroundAllocation = null)
    {
        return new RuleSetOperationResult(
            true,
            null,
            findings,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["attributeBudgets"] = FormatBudgets(draft.AttributeBudgets),
                ["attributeCategoryTotals"] = FormatAttributeCategoryTotals(attributeCategoryTotals ?? []),
                ["attributes"] = FormatNullableAttributes(draft.Attributes),
                ["attributePriorityOrder"] = string.Join(",", draft.AttributePriorityOrder),
                ["abilityBudgets"] = FormatBudgets(draft.AbilityBudgets),
                ["abilityCategoryTotals"] = FormatAbilityCategoryTotals(abilityCategoryTotals ?? []),
                ["abilities"] = FormatNullableRatings(draft.Abilities),
                ["abilityPriorityOrder"] = string.Join(",", draft.AbilityPriorityOrder),
                ["auspiceGiftId"] = draft.AuspiceGift ?? string.Empty,
                ["auspiceId"] = draft.Auspice ?? string.Empty,
                ["backgroundTotal"] = backgroundAllocation is null
                    ? string.Empty
                    : FormatBackgroundTotal(backgroundAllocation.Spent, backgroundAllocation.Budget, backgroundAllocation.Remaining),
                ["backgrounds"] = FormatNullableRatings(draft.Backgrounds),
                ["draftId"] = draft.DraftIdentity.Value,
                ["draftVersion"] = draft.DraftVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["identityName"] = draft.IdentityName ?? string.Empty,
                ["metisDeformityId"] = draft.MetisDeformity ?? string.Empty,
                ["nextSteps"] = string.Join(",", draft.RequiredNextSteps),
                ["raceGiftId"] = draft.RaceGift ?? string.Empty,
                ["raceId"] = draft.Race ?? string.Empty,
                ["rankId"] = draft.Rank ?? string.Empty,
                ["rankValue"] = draft.RankValue?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["renown"] = FormatNullableRatings(draft.Renown),
                ["resources"] = FormatNullableRatings(draft.Resources),
                ["tribeGiftId"] = draft.TribeGift ?? string.Empty,
                ["tribeId"] = draft.Tribe ?? string.Empty
            });
    }

    private static string[] ParseCsv(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static Dictionary<string, int> ParseBudgets(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new Dictionary<string, int>(StringComparer.Ordinal);
        }

        var values = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var entry in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = entry.Split(':', StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && int.TryParse(parts[1], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var budget))
            {
                values[parts[0]] = budget;
            }
        }

        return values;
    }

    private static List<WerewolfAttributeDotAllocation> ParseAttributeAllocations(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        var allocations = new List<WerewolfAttributeDotAllocation>();
        foreach (var entry in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = entry.Split(':', StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && int.TryParse(parts[1], System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture, out var rating))
            {
                allocations.Add(new WerewolfAttributeDotAllocation(parts[0], rating));
            }
            else
            {
                allocations.Add(new WerewolfAttributeDotAllocation(entry, 0));
            }
        }

        return allocations;
    }

    private static List<WerewolfAbilityDotAllocation> ParseAbilityAllocations(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        var allocations = new List<WerewolfAbilityDotAllocation>();
        foreach (var entry in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = entry.Split(':', StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && int.TryParse(parts[1], System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture, out var rating))
            {
                allocations.Add(new WerewolfAbilityDotAllocation(parts[0], rating));
            }
            else
            {
                allocations.Add(new WerewolfAbilityDotAllocation(entry, -1));
            }
        }

        return allocations;
    }

    private static List<WerewolfBackgroundRatingAllocation> ParseBackgroundAllocations(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        var allocations = new List<WerewolfBackgroundRatingAllocation>();
        foreach (var entry in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = entry.Split(':', StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && int.TryParse(parts[1], System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture, out var rating))
            {
                allocations.Add(new WerewolfBackgroundRatingAllocation(parts[0], rating));
            }
            else
            {
                allocations.Add(new WerewolfBackgroundRatingAllocation(entry, -1));
            }
        }

        return allocations;
    }

    private static System.Collections.ObjectModel.ReadOnlyDictionary<string, int?> ParseNullableAttributes(string? value)
    {
        return ParseNullableRatings(value);
    }

    private static System.Collections.ObjectModel.ReadOnlyDictionary<string, int?> ParseNullableRatings(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new System.Collections.ObjectModel.ReadOnlyDictionary<string, int?>(new Dictionary<string, int?>(StringComparer.Ordinal));
        }

        var values = new Dictionary<string, int?>(StringComparer.Ordinal);
        foreach (var entry in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = entry.Split(':', StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && int.TryParse(parts[1], System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture, out var rating))
            {
                values[parts[0]] = rating;
            }
        }

        return new System.Collections.ObjectModel.ReadOnlyDictionary<string, int?>(values);
    }

    private static string FormatNullableAttributes(IReadOnlyDictionary<string, int?> attributes)
    {
        return FormatNullableRatings(attributes);
    }

    private static string FormatNullableRatings(IReadOnlyDictionary<string, int?> attributes)
    {
        return string.Join(
            ",",
            attributes
                .Where(entry => entry.Value.HasValue)
                .OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .Select(entry => $"{entry.Key}:{entry.Value!.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));
    }

    private static string FormatBackgroundTotal(int spent, int budget, int remaining)
    {
        return $"{spent.ToString(System.Globalization.CultureInfo.InvariantCulture)}/{budget.ToString(System.Globalization.CultureInfo.InvariantCulture)}/{remaining.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
    }

    private static string FormatAttributeCategoryTotals(IReadOnlyList<WerewolfAttributeAllocationCategoryTotal> totals)
    {
        return string.Join(
            ",",
            totals
                .OrderBy(total => total.CategoryId, StringComparer.Ordinal)
                .Select(total => $"{total.CategoryId}:{total.Spent.ToString(System.Globalization.CultureInfo.InvariantCulture)}/{total.Budget.ToString(System.Globalization.CultureInfo.InvariantCulture)}/{total.Remaining.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));
    }

    private static string FormatAbilityCategoryTotals(IReadOnlyList<WerewolfAbilityAllocationCategoryTotal> totals)
    {
        return string.Join(
            ",",
            totals
                .OrderBy(total => total.CategoryId, StringComparer.Ordinal)
                .Select(total => $"{total.CategoryId}:{total.Spent.ToString(System.Globalization.CultureInfo.InvariantCulture)}/{total.Budget.ToString(System.Globalization.CultureInfo.InvariantCulture)}/{total.Remaining.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));
    }

    private static string FormatBudgets(IReadOnlyDictionary<string, int> budgets)
    {
        return string.Join(
            ",",
            budgets
                .OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .Select(entry => $"{entry.Key}:{entry.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));
    }

    private static RuleSetOperationResult ExecuteDefineActionTest(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("requestId", out var actionRequestId) ||
            !request.Inputs.TryGetValue("attributeId", out var attributeId) ||
            !request.Inputs.TryGetValue("abilityId", out var abilityId) ||
            !request.Inputs.TryGetValue("difficulty", out var difficultyText) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
            !int.TryParse(difficultyText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var difficulty))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidActionTestDefinitionRequest", "Action test definition requires draftId, draftVersion, expectedDraftVersion, requestId, attributeId, abilityId, and difficulty.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var modifierText = request.Inputs.GetValueOrDefault("modifier");
        var modifier = int.TryParse(modifierText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var parsedModifier)
            ? parsedModifier
            : (int?)null;

        var currentFormText = request.Inputs.GetValueOrDefault("currentForm");

        var draft = BuildDraftFromInputs(request, draftId, draftVersion);
        var result = WerewolfActionTestDefinitionService.DefineTest(new WerewolfActionTestDefinitionRequest(
            draft,
            expectedVersion,
            actionRequestId,
            attributeId,
            abilityId,
            difficulty,
            modifier,
            currentFormText));

        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(
                        finding.Severity == WerewolfActionTestDefinitionFindingSeverity.Error
                            ? RuleSetRuntimeFindingSeverity.Error
                            : RuleSetRuntimeFindingSeverity.Information,
                        finding.Code,
                        finding.Message))
                    .ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(
                    finding.Severity == WerewolfActionTestDefinitionFindingSeverity.Error
                        ? RuleSetRuntimeFindingSeverity.Error
                        : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code,
                    finding.Message))
                .ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = result.RequestId ?? string.Empty,
                ["diceQuantity"] = result.DiceQuantity?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["diceFaces"] = result.DiceFaces?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["attributeId"] = result.AttributeId ?? string.Empty,
                ["abilityId"] = result.AbilityId ?? string.Empty,
                ["difficulty"] = result.Difficulty?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["modifier"] = result.Modifier?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
            });
    }

    private static RuleSetOperationResult ExecuteInterpretActionRoll(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("requestId", out var actionRequestId) ||
            !request.Inputs.TryGetValue("diceValues", out var diceValuesText) ||
            !request.Inputs.TryGetValue("difficulty", out var difficultyText) ||
            !request.Inputs.TryGetValue("diceQuantity", out var diceQuantityText) ||
            !int.TryParse(difficultyText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var difficulty) ||
            !int.TryParse(diceQuantityText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var diceQuantity))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidActionRollInterpretationRequest", "Interpretation requires requestId, diceValues, difficulty, and diceQuantity.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var diceValues = ParseCsv(diceValuesText)
            .Select(value => int.TryParse(value, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var die) ? die : 0)
            .ToArray();

        var result = WerewolfActionRollInterpretationService.Interpret(new WerewolfActionRollInterpretationRequest(
            actionRequestId,
            Array.AsReadOnly(diceValues),
            difficulty,
            diceQuantity));

        if (!result.Succeeded)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(
                        finding.Severity == WerewolfActionRollInterpretationFindingSeverity.Error
                            ? RuleSetRuntimeFindingSeverity.Error
                            : RuleSetRuntimeFindingSeverity.Information,
                        finding.Code,
                        finding.Message))
                    .ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(
                    finding.Severity == WerewolfActionRollInterpretationFindingSeverity.Error
                        ? RuleSetRuntimeFindingSeverity.Error
                        : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code,
                    finding.Message))
                .ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = result.RequestId,
                ["rawDiceValues"] = string.Join(",", result.RawDiceValues.Select(d => d.ToString(System.Globalization.CultureInfo.InvariantCulture))),
                ["diceQuantity"] = result.DiceQuantity.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["difficulty"] = result.Difficulty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["successCount"] = result.SuccessCount?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["failureClassification"] = result.FailureClassification ?? string.Empty,
                ["botchClassification"] = result.BotchClassification ?? string.Empty,
                ["interpretationStatus"] = result.InterpretationStatus,
                ["serializedInterpretation"] = result.SerializedInterpretation
            });
    }

    private static RuleSetOperationResult ExecuteDefineExtendedTest(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
            !request.Inputs.TryGetValue("dicePool", out var dicePoolText) ||
            !request.Inputs.TryGetValue("difficulty", out var difficultyText) ||
            !request.Inputs.TryGetValue("requiredSuccesses", out var requiredSuccessesText) ||
            !int.TryParse(dicePoolText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var dicePool) ||
            !int.TryParse(difficultyText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var difficulty) ||
            !int.TryParse(requiredSuccessesText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var requiredSuccesses))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidExtendedTestDefinitionRequest", "Extended test definition requires requestId, dicePool, difficulty, and requiredSuccesses.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var definition = new WerewolfExtendedTestDefinition(requestId, dicePool, difficulty, requiredSuccesses);
        var progress = WerewolfExtendedTestService.CreateInitialProgress(definition);

        return new RuleSetOperationResult(
            true,
            null,
            [],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = progress.RequestId,
                ["dicePool"] = dicePool.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["difficulty"] = difficulty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["requiredSuccesses"] = requiredSuccesses.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["diceFaces"] = "10",
                ["accumulatedSuccesses"] = progress.AccumulatedSuccesses.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["attemptCount"] = progress.AttemptCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["isBotched"] = progress.IsBotched.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["status"] = progress.Status.ToString()
            });
    }

    private static RuleSetOperationResult ExecuteAdvanceExtendedTest(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
            !request.Inputs.TryGetValue("dicePool", out var dicePoolText) ||
            !request.Inputs.TryGetValue("difficulty", out var difficultyText) ||
            !request.Inputs.TryGetValue("requiredSuccesses", out var requiredSuccessesText) ||
            !request.Inputs.TryGetValue("diceValues", out var diceValuesText) ||
            !request.Inputs.TryGetValue("previousProgressJson", out var previousProgressJson) ||
            !int.TryParse(dicePoolText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var dicePool) ||
            !int.TryParse(difficultyText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var difficulty) ||
            !int.TryParse(requiredSuccessesText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var requiredSuccesses))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidExtendedTestAdvanceRequest", "Extended test advance requires requestId, dicePool, difficulty, requiredSuccesses, diceValues, and previousProgressJson.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        WerewolfExtendedTestProgress? previousProgress = null;
        if (!string.IsNullOrWhiteSpace(previousProgressJson))
        {
            try
            {
                previousProgress = System.Text.Json.JsonSerializer.Deserialize<WerewolfExtendedTestProgress>(previousProgressJson, JsonOptions);
            }
            catch
            {
            }
        }

        if (previousProgress is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidPreviousProgress", "previousProgressJson must be a valid WerewolfExtendedTestProgress.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var definition = new WerewolfExtendedTestDefinition(requestId, dicePool, difficulty, requiredSuccesses);
        var diceValues = ParseCsv(diceValuesText)
            .Select(value => int.TryParse(value, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var die) ? die : 0)
            .ToArray();

        var result = WerewolfExtendedTestService.Advance(definition, previousProgress, Array.AsReadOnly(diceValues));

        if (!result.Succeeded)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(
                        finding.Severity == WerewolfExtendedTestFindingSeverity.Error
                            ? RuleSetRuntimeFindingSeverity.Error
                            : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code,
                    finding.Message))
                    .ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(
                    finding.Severity == WerewolfExtendedTestFindingSeverity.Error
                        ? RuleSetRuntimeFindingSeverity.Error
                        : RuleSetRuntimeFindingSeverity.Information,
                finding.Code,
                finding.Message))
            .ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = result.RequestId,
                ["accumulatedSuccesses"] = result.UpdatedProgress.AccumulatedSuccesses.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["attemptCount"] = result.UpdatedProgress.AttemptCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["isBotched"] = result.UpdatedProgress.IsBotched.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["status"] = result.Status.ToString(),
                ["serializedResult"] = result.SerializedResult
            });
    }

    private static RuleSetOperationResult ExecuteDefineResistedTest(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
            !request.Inputs.TryGetValue("sideADicePool", out var sideADicePoolText) ||
            !request.Inputs.TryGetValue("sideADifficulty", out var sideADifficultyText) ||
            !request.Inputs.TryGetValue("sideBDicePool", out var sideBDicePoolText) ||
            !request.Inputs.TryGetValue("sideBDifficulty", out var sideBDifficultyText) ||
            !int.TryParse(sideADicePoolText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var sideADicePool) ||
            !int.TryParse(sideADifficultyText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var sideADifficulty) ||
            !int.TryParse(sideBDicePoolText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var sideBDicePool) ||
            !int.TryParse(sideBDifficultyText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var sideBDifficulty))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidResistedTestDefinitionRequest", "Resisted test definition requires requestId, sideADicePool, sideADifficulty, sideBDicePool, and sideBDifficulty.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var definition = new WerewolfResistedTestDefinition(requestId, sideADicePool, sideADifficulty, sideBDicePool, sideBDifficulty);

        return new RuleSetOperationResult(
            true,
            null,
            [],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = definition.RequestId,
                ["sideADicePool"] = sideADicePool.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["sideADifficulty"] = sideADifficulty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["sideBDicePool"] = sideBDicePool.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["sideBDifficulty"] = sideBDifficulty.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
    }

    private static RuleSetOperationResult ExecuteInterpretResistedTest(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
            !request.Inputs.TryGetValue("sideADiceValues", out var sideADiceValuesText) ||
            !request.Inputs.TryGetValue("sideBDiceValues", out var sideBDiceValuesText) ||
            !request.Inputs.TryGetValue("sideADifficulty", out var sideADifficultyText) ||
            !request.Inputs.TryGetValue("sideBDifficulty", out var sideBDifficultyText) ||
            !request.Inputs.TryGetValue("sideADicePool", out var sideADicePoolText) ||
            !request.Inputs.TryGetValue("sideBDicePool", out var sideBDicePoolText) ||
            !int.TryParse(sideADifficultyText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var sideADifficulty) ||
            !int.TryParse(sideBDifficultyText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var sideBDifficulty) ||
            !int.TryParse(sideADicePoolText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var sideADicePool) ||
            !int.TryParse(sideBDicePoolText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var sideBDicePool))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidResistedTestInterpretationRequest", "Resisted test interpretation requires requestId, sideADiceValues, sideBDiceValues, sideADifficulty, sideBDifficulty, sideADicePool, and sideBDicePool.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var sideADiceValues = ParseCsv(sideADiceValuesText)
            .Select(value => int.TryParse(value, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var die) ? die : 0)
            .ToArray();

        var sideBDiceValues = ParseCsv(sideBDiceValuesText)
            .Select(value => int.TryParse(value, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var die) ? die : 0)
            .ToArray();

        var definition = new WerewolfResistedTestDefinition(requestId, sideADicePool, sideADifficulty, sideBDicePool, sideBDifficulty);
        var result = WerewolfResistedTestService.Interpret(definition, Array.AsReadOnly(sideADiceValues), Array.AsReadOnly(sideBDiceValues));

        if (!result.Succeeded)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(
                        finding.Severity == WerewolfResistedTestFindingSeverity.Error
                            ? RuleSetRuntimeFindingSeverity.Error
                            : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code,
                    finding.Message))
                    .ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(
                    finding.Severity == WerewolfResistedTestFindingSeverity.Error
                        ? RuleSetRuntimeFindingSeverity.Error
                        : RuleSetRuntimeFindingSeverity.Information,
                finding.Code,
                finding.Message))
            .ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = result.RequestId,
                ["sideASuccesses"] = result.SideASuccesses.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["sideBSuccesses"] = result.SideBSuccesses.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["netSuccesses"] = result.NetSuccesses.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["winner"] = result.Winner.ToString(),
                ["status"] = result.Status,
                ["serializedResult"] = result.SerializedResult
            });
    }

    private static RuleSetOperationResult ExecuteSpendResource(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
            !request.Inputs.TryGetValue("currentState", out var currentStateText) ||
            !request.Inputs.TryGetValue("expectedRuntimeStateVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("resourceId", out var resourceId) ||
            !request.Inputs.TryGetValue("amount", out var amountText) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
            !int.TryParse(amountText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var amount))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidSpendResourceRequest", "Spend-resource requires requestId, currentState, expectedRuntimeStateVersion, resourceId, and amount.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentState = System.Text.Json.JsonSerializer.Deserialize<WerewolfRuntimeCharacterState>(currentStateText, JsonOptions);
        if (currentState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidCurrentState", "Current state is not a valid WerewolfRuntimeCharacterState.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var result = WerewolfResourceTransitionService.Spend(new WerewolfResourceTransitionRequest(currentState, expectedVersion, requestId, resourceId, amount));

        if (!result.Succeeded || result.NewState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(
                        finding.Severity == WerewolfResourceTransitionFindingSeverity.Error
                            ? RuleSetRuntimeFindingSeverity.Error
                            : RuleSetRuntimeFindingSeverity.Information,
                        finding.Code.ToString(),
                        finding.Message))
                    .ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return ToOperationResult(result);
    }

    private static RuleSetOperationResult ExecuteRecoverResource(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
            !request.Inputs.TryGetValue("currentState", out var currentStateText) ||
            !request.Inputs.TryGetValue("expectedRuntimeStateVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("resourceId", out var resourceId) ||
            !request.Inputs.TryGetValue("amount", out var amountText) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
            !int.TryParse(amountText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var amount))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidRecoverResourceRequest", "Recover-resource requires requestId, currentState, expectedRuntimeStateVersion, resourceId, and amount.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentState = System.Text.Json.JsonSerializer.Deserialize<WerewolfRuntimeCharacterState>(currentStateText, JsonOptions);
        if (currentState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidCurrentState", "Current state is not a valid WerewolfRuntimeCharacterState.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var result = WerewolfResourceTransitionService.Recover(new WerewolfResourceTransitionRequest(currentState, expectedVersion, requestId, resourceId, amount));

        if (!result.Succeeded || result.NewState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(
                        finding.Severity == WerewolfResourceTransitionFindingSeverity.Error
                            ? RuleSetRuntimeFindingSeverity.Error
                            : RuleSetRuntimeFindingSeverity.Information,
                        finding.Code.ToString(),
                        finding.Message))
                    .ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return ToOperationResult(result);
    }

    private static RuleSetOperationResult ToOperationResult(WerewolfResourceTransitionResult result)
    {
        var newState = result.NewState!;
        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(
                    finding.Severity == WerewolfResourceTransitionFindingSeverity.Error
                        ? RuleSetRuntimeFindingSeverity.Error
                        : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code.ToString(),
                    finding.Message))
                .ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = result.RequestId ?? string.Empty,
                ["newState"] = System.Text.Json.JsonSerializer.Serialize(newState, JsonOptions),
                ["newRuntimeStateVersion"] = newState.RuntimeStateVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["previousCurrent"] = result.PreviousCurrent?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["newCurrent"] = result.NewCurrent?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["previousPermanent"] = result.PreviousPermanent?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["newPermanent"] = result.NewPermanent?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
            });
    }

    private static RuleSetOperationResult ExecuteAwardTemporaryRenown(RuleSetOperationRequest request)
    {
        var currentState = GetCurrentRuntimeState(request);
        var expectedVersion = GetExpectedVersion(request);
        var requestId = request.Inputs.GetValueOrDefault("requestId", string.Empty);
        var renownId = request.Inputs.GetValueOrDefault("renownId", string.Empty);
        var amount = ParseInt(request.Inputs.GetValueOrDefault("amount", "0"));

        var result = WerewolfRenownTransitionService.AwardTemporaryRenown(new WerewolfRenownTransitionRequest(currentState, expectedVersion, requestId, renownId, amount, false));

        if (!result.Succeeded || result.NewState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(
                        finding.Severity == WerewolfRenownTransitionFindingSeverity.Error
                            ? RuleSetRuntimeFindingSeverity.Error
                            : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code.ToString(),
                    finding.Message))
                    .ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return ToRenownOperationResult(result);
    }

    private static RuleSetOperationResult ExecuteLoseTemporaryRenown(RuleSetOperationRequest request)
    {
        var currentState = GetCurrentRuntimeState(request);
        var expectedVersion = GetExpectedVersion(request);
        var requestId = request.Inputs.GetValueOrDefault("requestId", string.Empty);
        var renownId = request.Inputs.GetValueOrDefault("renownId", string.Empty);
        var amount = ParseInt(request.Inputs.GetValueOrDefault("amount", "0"));

        var result = WerewolfRenownTransitionService.LoseTemporaryRenown(new WerewolfRenownTransitionRequest(currentState, expectedVersion, requestId, renownId, amount, false));

        if (!result.Succeeded || result.NewState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(
                        finding.Severity == WerewolfRenownTransitionFindingSeverity.Error
                            ? RuleSetRuntimeFindingSeverity.Error
                            : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code.ToString(),
                    finding.Message))
                    .ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return ToRenownOperationResult(result);
    }

    private static RuleSetOperationResult ExecuteConvertTemporaryToPermanentRenown(RuleSetOperationRequest request)
    {
        var currentState = GetCurrentRuntimeState(request);
        var expectedVersion = GetExpectedVersion(request);
        var requestId = request.Inputs.GetValueOrDefault("requestId", string.Empty);
        var renownId = request.Inputs.GetValueOrDefault("renownId", string.Empty);

        var result = WerewolfRenownTransitionService.ConvertTemporaryToPermanent(new WerewolfRenownTransitionRequest(currentState, expectedVersion, requestId, renownId, WerewolfRenownTransitionService.TemporaryToPermanentThreshold, false));

        if (!result.Succeeded || result.NewState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(
                        finding.Severity == WerewolfRenownTransitionFindingSeverity.Error
                            ? RuleSetRuntimeFindingSeverity.Error
                            : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code.ToString(),
                    finding.Message))
                    .ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return ToRenownOperationResult(result);
    }

    private static RuleSetOperationResult ToRenownOperationResult(WerewolfRenownTransitionResult result)
    {
        var newState = result.NewState!;
        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(
                    finding.Severity == WerewolfRenownTransitionFindingSeverity.Error
                        ? RuleSetRuntimeFindingSeverity.Error
                        : RuleSetRuntimeFindingSeverity.Information,
                finding.Code.ToString(),
                finding.Message))
            .ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = result.RequestId ?? string.Empty,
                ["newState"] = System.Text.Json.JsonSerializer.Serialize(newState, JsonOptions),
                ["newRuntimeStateVersion"] = newState.RuntimeStateVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["previousCurrent"] = result.PreviousCurrent?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["newCurrent"] = result.NewCurrent?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["previousPermanent"] = result.PreviousPermanent?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["newPermanent"] = result.NewPermanent?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
            });
    }

    private static RuleSetOperationResult ExecuteApplyDamage(RuleSetOperationRequest request)
    {
        var currentState = GetCurrentRuntimeState(request);
        var expectedVersion = GetExpectedVersion(request);
        var requestId = request.Inputs.GetValueOrDefault("requestId", string.Empty);
        var damageTypeText = request.Inputs.GetValueOrDefault("damageType", string.Empty);
        var amountText = request.Inputs.GetValueOrDefault("amount", string.Empty);

        if (!Enum.TryParse<WerewolfDamageCategory>(damageTypeText, true, out var damageType))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidDamageType", $"Invalid damage type: {damageTypeText}")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        if (!int.TryParse(amountText, out var amount) || amount <= 0)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidAmount", "Amount must be a positive integer")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var result = WerewolfApplyDamageService.ApplyDamage(new WerewolfApplyDamageRequest(
            requestId,
            currentState,
            expectedVersion,
            damageType,
            amount));

        if (!result.Succeeded || result.UpdatedState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, result.ErrorCode ?? "ApplyDamageFailed", f)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "ApplyDamageSucceeded", f)).ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = result.RequestId,
                ["newState"] = System.Text.Json.JsonSerializer.Serialize(result.UpdatedState, JsonOptions),
                ["newRuntimeStateVersion"] = result.UpdatedState.RuntimeStateVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["healthTrack"] = System.Text.Json.JsonSerializer.Serialize(result.HealthTrack, JsonOptions),
                ["currentLevel"] = result.HealthTrack.CurrentLevel.ToString(),
                ["woundPenalty"] = result.HealthTrack.WoundPenalty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["healthState"] = result.HealthTrack.HealthState.ToString(),
                ["fatalDamageType"] = result.HealthTrack.FatalDamageType?.ToString() ?? string.Empty,
                ["isIncapacitated"] = (result.HealthTrack.HealthState == WerewolfHealthState.Incapacitated).ToString(),
                ["isUnconscious"] = (result.HealthTrack.HealthState == WerewolfHealthState.Unconscious).ToString(),
                ["isNearDeath"] = (result.HealthTrack.HealthState == WerewolfHealthState.NearDeath).ToString(),
                ["isDead"] = (result.HealthTrack.HealthState == WerewolfHealthState.Dead).ToString(),
                ["totalDamage"] = result.HealthTrack.TotalDamage.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
    }

    private static RuleSetOperationResult ExecuteRecoverDamage(RuleSetOperationRequest request)
    {
        var currentState = GetCurrentRuntimeState(request);
        var expectedVersion = GetExpectedVersion(request);
        var requestId = request.Inputs.GetValueOrDefault("requestId", string.Empty);
        var damageTypeText = request.Inputs.GetValueOrDefault("damageType", string.Empty);
        var amountText = request.Inputs.GetValueOrDefault("amount", string.Empty);
        var requiresAlternateFormRestText = request.Inputs.GetValueOrDefault("requiresAlternateFormRest", "false");

        if (!Enum.TryParse<WerewolfDamageCategory>(damageTypeText, true, out var damageType))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidDamageType", $"Invalid damage type: {damageTypeText}")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        if (!int.TryParse(amountText, out var amount) || amount <= 0)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidAmount", "Amount must be a positive integer")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        if (!bool.TryParse(requiresAlternateFormRestText, out var requiresAlternateFormRest))
        {
            requiresAlternateFormRest = false;
        }

        var result = WerewolfRecoverDamageService.RecoverDamage(new WerewolfRecoverDamageRequest(
            requestId,
            currentState,
            expectedVersion,
            damageType,
            amount,
            requiresAlternateFormRest));

        if (!result.Succeeded || result.UpdatedState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, result.ErrorCode ?? "RecoverDamageFailed", f)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "RecoverDamageSucceeded", f)).ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = result.RequestId,
                ["newState"] = System.Text.Json.JsonSerializer.Serialize(result.UpdatedState, JsonOptions),
                ["newRuntimeStateVersion"] = result.UpdatedState.RuntimeStateVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["healthTrack"] = System.Text.Json.JsonSerializer.Serialize(result.HealthTrack, JsonOptions),
                ["currentLevel"] = result.HealthTrack.CurrentLevel.ToString(),
                ["woundPenalty"] = result.HealthTrack.WoundPenalty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["healthState"] = result.HealthTrack.HealthState.ToString(),
                ["fatalDamageType"] = result.HealthTrack.FatalDamageType?.ToString() ?? string.Empty,
                ["isIncapacitated"] = (result.HealthTrack.HealthState == WerewolfHealthState.Incapacitated).ToString(),
                ["isUnconscious"] = (result.HealthTrack.HealthState == WerewolfHealthState.Unconscious).ToString(),
                ["isNearDeath"] = (result.HealthTrack.HealthState == WerewolfHealthState.NearDeath).ToString(),
                ["isDead"] = (result.HealthTrack.HealthState == WerewolfHealthState.Dead).ToString(),
                ["totalDamage"] = result.HealthTrack.TotalDamage.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
    }

    private static RuleSetOperationResult ExecuteRegenerate(RuleSetOperationRequest request)
    {
        var currentState = GetCurrentRuntimeState(request);
        var expectedVersion = GetExpectedVersion(request);
        var requestId = request.Inputs.GetValueOrDefault("requestId", string.Empty);
        var damageTypeText = request.Inputs.GetValueOrDefault("damageType", string.Empty);
         var amountText = request.Inputs.GetValueOrDefault("amount", string.Empty);
         var currentTurnText = request.Inputs.GetValueOrDefault("currentTurn", "0");
         var isStressfulText = request.Inputs.GetValueOrDefault("isStressful", "false");
        var requiresAlternateFormRestText = request.Inputs.GetValueOrDefault("requiresAlternateFormRest", "false");
        var vigorDicePoolText = request.Inputs.GetValueOrDefault("vigorDicePool", "0");
        var vigorSuccessesText = request.Inputs.GetValueOrDefault("vigorSuccesses", "0");
        var vigorOnesText = request.Inputs.GetValueOrDefault("vigorOnes", "0");

        if (!Enum.TryParse<WerewolfDamageCategory>(damageTypeText, true, out var damageType))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidDamageType", $"Invalid damage type: {damageTypeText}")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        if (!int.TryParse(amountText, out var amount) || amount <= 0)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidAmount", "Amount must be a positive integer")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        if (!int.TryParse(currentTurnText, out var currentTurn))
        {
            currentTurn = 0;
        }

        if (!bool.TryParse(isStressfulText, out var isStressful))
        {
            isStressful = false;
        }

        if (!bool.TryParse(requiresAlternateFormRestText, out var requiresAlternateFormRest))
        {
            requiresAlternateFormRest = false;
        }

        if (!int.TryParse(vigorDicePoolText, out var vigorDicePool))
        {
            vigorDicePool = 0;
        }

        if (!int.TryParse(vigorSuccessesText, out var vigorSuccesses))
        {
            vigorSuccesses = 0;
        }

        if (!int.TryParse(vigorOnesText, out var vigorOnes))
        {
            vigorOnes = 0;
        }

        var result = WerewolfRegenerationService.Regenerate(new WerewolfRegenerationRequest(
            requestId,
            currentState,
            expectedVersion,
            damageType,
            amount,
            currentTurn,
            isStressful,
            requiresAlternateFormRest,
            vigorDicePool,
            vigorSuccesses,
            vigorOnes));

        if (!result.Succeeded || result.UpdatedState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, result.ErrorCode ?? "RegenerateFailed", f)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "RegenerateSucceeded", f)).ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = result.RequestId,
                ["newState"] = System.Text.Json.JsonSerializer.Serialize(result.UpdatedState, JsonOptions),
                ["newRuntimeStateVersion"] = result.UpdatedState.RuntimeStateVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["healthTrack"] = System.Text.Json.JsonSerializer.Serialize(result.HealthTrack, JsonOptions),
                ["currentLevel"] = result.HealthTrack.CurrentLevel.ToString(),
                ["woundPenalty"] = result.HealthTrack.WoundPenalty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["healthState"] = result.HealthTrack.HealthState.ToString(),
                ["fatalDamageType"] = result.HealthTrack.FatalDamageType?.ToString() ?? string.Empty,
                ["isIncapacitated"] = (result.HealthTrack.HealthState == WerewolfHealthState.Incapacitated).ToString(),
                ["isUnconscious"] = (result.HealthTrack.HealthState == WerewolfHealthState.Unconscious).ToString(),
                ["isNearDeath"] = (result.HealthTrack.HealthState == WerewolfHealthState.NearDeath).ToString(),
                ["isDead"] = (result.HealthTrack.HealthState == WerewolfHealthState.Dead).ToString(),
                ["totalDamage"] = result.HealthTrack.TotalDamage.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["successes"] = result.Successes?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
            });
    }

    private static RuleSetOperationResult ExecutePermanecerAtivo(RuleSetOperationRequest request)
    {
        var currentState = GetCurrentRuntimeState(request);
        var expectedVersion = GetExpectedVersion(request);
        var requestId = request.Inputs.GetValueOrDefault("requestId", string.Empty);
        var furySuccessesText = request.Inputs.GetValueOrDefault("furySuccesses", "0");
        var furyOnesText = request.Inputs.GetValueOrDefault("furyOnes", "0");

        if (!int.TryParse(furySuccessesText, out var furySuccesses))
        {
            furySuccesses = 0;
        }

        if (!int.TryParse(furyOnesText, out var furyOnes))
        {
            furyOnes = 0;
        }

        var result = WerewolfPermanecerAtivoService.PermanecerAtivo(new WerewolfPermanecerAtivoRequest(
            requestId,
            currentState,
            expectedVersion,
            furySuccesses,
            furyOnes));

        if (!result.Succeeded || result.UpdatedState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, result.ErrorCode ?? "PermanecerAtivoFailed", f)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "PermanecerAtivoSucceeded", f)).ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = result.RequestId,
                ["newState"] = System.Text.Json.JsonSerializer.Serialize(result.UpdatedState, JsonOptions),
                ["newRuntimeStateVersion"] = result.UpdatedState.RuntimeStateVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["healthTrack"] = System.Text.Json.JsonSerializer.Serialize(result.HealthTrack, JsonOptions),
                ["currentLevel"] = result.HealthTrack.CurrentLevel.ToString(),
                ["woundPenalty"] = result.HealthTrack.WoundPenalty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["healthState"] = result.HealthTrack.HealthState.ToString(),
                ["fatalDamageType"] = result.HealthTrack.FatalDamageType?.ToString() ?? string.Empty,
                ["isIncapacitated"] = (result.HealthTrack.HealthState == WerewolfHealthState.Incapacitated).ToString(),
                ["isUnconscious"] = (result.HealthTrack.HealthState == WerewolfHealthState.Unconscious).ToString(),
                ["isNearDeath"] = (result.HealthTrack.HealthState == WerewolfHealthState.NearDeath).ToString(),
                ["isDead"] = (result.HealthTrack.HealthState == WerewolfHealthState.Dead).ToString(),
                ["isSurvived"] = (result.HealthTrack.HealthState == WerewolfHealthState.Survived).ToString(),
                ["totalDamage"] = result.HealthTrack.TotalDamage.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["successes"] = result.Successes?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
            });
    }

    private static WerewolfRuntimeCharacterState GetCurrentRuntimeState(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("newState", out var newStateJson) || string.IsNullOrWhiteSpace(newStateJson))
        {
            throw new ArgumentException("Runtime Renown transition requires a current state payload.", nameof(request));
        }

        var state = System.Text.Json.JsonSerializer.Deserialize<WerewolfRuntimeCharacterState>(newStateJson, JsonOptions);
        if (state is null)
        {
            throw new ArgumentException("Runtime Renown transition state payload is invalid.", nameof(request));
        }

        return state;
    }

    private static int GetExpectedVersion(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("expectedRuntimeStateVersion", out var versionString) || !int.TryParse(versionString, out var version))
        {
            throw new ArgumentException("Runtime Renown transition requires expected runtime state version.", nameof(request));
        }

        return version;
    }

    private static int ParseInt(string value)
    {
        if (int.TryParse(value, out var result))
        {
            return result;
        }

        return 0;
    }

    private static RuleSetOperationResult ExecuteDefineInitiative(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("currentState", out var currentStateText) ||
            !request.Inputs.TryGetValue("expectedRuntimeStateVersion", out var expectedVersionText) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidInitiativeRequest", "Initiative requires currentState and expectedRuntimeStateVersion.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentState = System.Text.Json.JsonSerializer.Deserialize<WerewolfRuntimeCharacterState>(currentStateText, JsonOptions);
        if (currentState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidCurrentState", "Current state is not valid.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var baseAttributes = request.Inputs.TryGetValue("baseAttributes", out var baseAttributesText)
            ? ParseNullableRatings(baseAttributesText)
            : new System.Collections.ObjectModel.ReadOnlyDictionary<string, int?>(new Dictionary<string, int?>(StringComparer.Ordinal));
        var attributes = WerewolfEffectiveAttributeService.ComputeEffectiveAttributes(baseAttributes, currentState.CurrentForm);

        var pool = WerewolfCombatInitiativeService.ComputeInitiativeModifier(attributes);
        var turnStructure = WerewolfCombatInitiativeService.GetTurnStructure();

        return new RuleSetOperationResult(
            true,
            null,
            [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "InitiativeComputed", $"Initiative modifier computed as {pool}.")],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["initiativeModifier"] = pool.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["turnStructure"] = string.Join(",", turnStructure),
                ["turnDuration"] = WerewolfCombatInitiativeService.GetTurnDuration()
            });
    }

    private static RuleSetOperationResult ExecuteDefineAttack(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("attackId", out var attackId) ||
            !request.Inputs.TryGetValue("currentState", out var currentStateText) ||
            !request.Inputs.TryGetValue("expectedRuntimeStateVersion", out var expectedVersionText) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidAttackRequest", "Attack definition requires attackId, currentState, and expectedRuntimeStateVersion.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentState = System.Text.Json.JsonSerializer.Deserialize<WerewolfRuntimeCharacterState>(currentStateText, JsonOptions);
        if (currentState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidCurrentState", "Current state is not valid.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        try
        {
            var definition = WerewolfCombatAttackDefinitionService.ResolveAttack(attackId);
            var baseAttributes = request.Inputs.TryGetValue("baseAttributes", out var baseAttributesText)
                ? ParseNullableRatings(baseAttributesText)
                : new System.Collections.ObjectModel.ReadOnlyDictionary<string, int?>(new Dictionary<string, int?>(StringComparer.Ordinal));
            var attributes = WerewolfEffectiveAttributeService.ComputeEffectiveAttributes(baseAttributes, currentState.CurrentForm);

            var pool = WerewolfCombatDefenseService.ComputeDefensePool(attributes, attackId);

            return new RuleSetOperationResult(
                true,
                null,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "AttackDefined", $"Attack {attackId} defined with pool {pool}.")],
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["attackId"] = attackId,
                    ["attackPool"] = pool.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["damageType"] = definition.DamageType ?? string.Empty,
                    ["notes"] = definition.Notes ?? string.Empty
                });
        }
        catch (ArgumentException ex)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidAttackId", ex.Message)],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }
    }

    private static RuleSetOperationResult ExecuteDefineDefense(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("defenseId", out var defenseId) ||
            !request.Inputs.TryGetValue("currentState", out var currentStateText) ||
            !request.Inputs.TryGetValue("expectedRuntimeStateVersion", out var expectedVersionText) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidDefenseRequest", "Defense definition requires defenseId, currentState, and expectedRuntimeStateVersion.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentState = System.Text.Json.JsonSerializer.Deserialize<WerewolfRuntimeCharacterState>(currentStateText, JsonOptions);
        if (currentState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidCurrentState", "Current state is not valid.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        try
        {
            var definition = WerewolfCombatDefenseService.ResolveDefense(defenseId);
            var baseAttributes = request.Inputs.TryGetValue("baseAttributes", out var baseAttributesText)
                ? ParseNullableRatings(baseAttributesText)
                : new System.Collections.ObjectModel.ReadOnlyDictionary<string, int?>(new Dictionary<string, int?>(StringComparer.Ordinal));
            var attributes = WerewolfEffectiveAttributeService.ComputeEffectiveAttributes(baseAttributes, currentState.CurrentForm);

            var pool = WerewolfCombatDefenseService.ComputeDefensePool(attributes, defenseId);

            return new RuleSetOperationResult(
                true,
                null,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "DefenseDefined", $"Defense {defenseId} defined with pool {pool}.")],
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["defenseId"] = defenseId,
                    ["defensePool"] = pool.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["effectiveAgainstFirearms"] = definition.IsEffectiveAgainstFirearms.ToString(),
                    ["notes"] = definition.Notes ?? string.Empty
                });
        }
        catch (ArgumentException ex)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidDefenseId", ex.Message)],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }
    }

    private static RuleSetOperationResult ExecuteCalculateDamage(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("currentState", out var currentStateText) ||
            !request.Inputs.TryGetValue("expectedRuntimeStateVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("attackSuccesses", out var successesText) ||
            !request.Inputs.TryGetValue("damageExpression", out var damageExpressionText) ||
            !request.Inputs.TryGetValue("damageCategory", out var damageCategoryText) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
            !int.TryParse(successesText, out var successes))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidDamageRequest", "Damage calculation requires currentState, expectedRuntimeStateVersion, attackSuccesses, damageExpression, and damageCategory.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentState = System.Text.Json.JsonSerializer.Deserialize<WerewolfRuntimeCharacterState>(currentStateText, JsonOptions);
        if (currentState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidCurrentState", "Current state is not valid.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var strengthBonusText = request.Inputs.TryGetValue("strengthBonus", out var sbText) && int.TryParse(sbText, out var sb) ? sb : (int?)null;

        var damageRequest = new WerewolfCombatDamageRequest(
            request.Inputs.GetValueOrDefault("requestId", string.Empty),
            currentState,
            expectedVersion,
            successes,
            damageExpressionText,
            damageCategoryText,
            strengthBonusText);

        var definition = WerewolfCombatDamageService.DefineDamageRoll(damageRequest);

        return new RuleSetOperationResult(
            true,
            null,
            [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "DamageDefined", string.Join("; ", definition.Findings))],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["damagePoolSize"] = definition.DamagePoolSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["difficulty"] = definition.Difficulty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["damageCategory"] = definition.DamageCategory,
                ["attackSuccesses"] = successes.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["findings"] = string.Join("; ", definition.Findings)
            });
    }

    private static RuleSetOperationResult ExecuteCalculateSoak(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("currentState", out var currentStateText) ||
            !request.Inputs.TryGetValue("expectedRuntimeStateVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("damageType", out var damageTypeText) ||
            !request.Inputs.TryGetValue("amount", out var amountText) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
            !int.TryParse(amountText, out var amount) ||
            !Enum.TryParse<WerewolfDamageCategory>(damageTypeText, true, out var damageType))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidSoakRequest", "Soak requires currentState, expectedRuntimeStateVersion, damageType, and amount.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentState = System.Text.Json.JsonSerializer.Deserialize<WerewolfRuntimeCharacterState>(currentStateText, JsonOptions);
        if (currentState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidCurrentState", "Current state is not valid.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var hasGiftsOrFetishes = request.Inputs.TryGetValue("hasGiftsOrFetishes", out var giftsText) &&
            bool.TryParse(giftsText, out var gifts) && gifts;
        var isImpure = StringComparer.Ordinal.Equals(currentState.BirthRace, WerewolfRaceIdentifiers.Metis);

        var soakRequest = new WerewolfCombatSoakRequest(
            request.Inputs.GetValueOrDefault("requestId", string.Empty),
            currentState,
            expectedVersion,
            damageType,
            amount);

        var soakDefinition = WerewolfCombatSoakService.DefineSoakRoll(soakRequest);

        return new RuleSetOperationResult(
            true,
            null,
            [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "SoakDefined", string.Join("; ", soakDefinition.Findings))],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["soakPoolSize"] = soakDefinition.SoakPoolSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["difficulty"] = soakDefinition.Difficulty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["isRacialForm"] = soakDefinition.IsRacialForm.ToString(),
                ["isSilver"] = soakDefinition.IsSilver.ToString(),
                ["soakBlocked"] = soakDefinition.SoakBlocked.ToString(),
                ["damageType"] = damageType.ToString(),
                ["incomingDamage"] = amount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["findings"] = string.Join("; ", soakDefinition.Findings)
            });
    }

    private static RuleSetOperationResult ExecuteApplySilver(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("currentState", out var currentStateText) ||
            !request.Inputs.TryGetValue("expectedRuntimeStateVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("turnsOfContact", out var turnsText) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
            !int.TryParse(turnsText, out var turnsOfContact))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidSilverRequest", "Silver application requires currentState, expectedRuntimeStateVersion, and turnsOfContact.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentState = System.Text.Json.JsonSerializer.Deserialize<WerewolfRuntimeCharacterState>(currentStateText, JsonOptions);
        if (currentState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidCurrentState", "Current state is not valid.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var hasGiftsOrFetishes = request.Inputs.TryGetValue("hasGiftsOrFetishes", out var giftsText) &&
            bool.TryParse(giftsText, out var gifts) && gifts;

        var silverDamage = WerewolfCombatSilverService.ApplySilverContact(currentState, turnsOfContact, hasGiftsOrFetishes);

        return new RuleSetOperationResult(
            true,
            null,
            [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "SilverApplied", $"Silver contact applied: {silverDamage} aggravated damage.")],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["totalAggravatedDamage"] = silverDamage.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["damageType"] = WerewolfDamageCategory.Aggravated.ToString(),
                ["findings"] = $"Silver contact: {silverDamage} aggravated damage."
            });
    }

    private static RuleSetOperationResult ExecuteApplyRage(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("currentState", out var currentStateText) ||
            !request.Inputs.TryGetValue("expectedRuntimeStateVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("rageInvested", out var rageText) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
            !int.TryParse(rageText, out var rageInvested))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidRageRequest", "Rage application requires currentState, expectedRuntimeStateVersion, and rageInvested.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentState = System.Text.Json.JsonSerializer.Deserialize<WerewolfRuntimeCharacterState>(currentStateText, JsonOptions);
        if (currentState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidCurrentState", "Current state is not valid.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var result = WerewolfCombatRageService.CalculateExtraActions(currentState, rageInvested);

        return new RuleSetOperationResult(
            true,
            null,
            [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "RageApplied", string.Join("; ", result.Findings))],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["extraActions"] = result.ExtraActions.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["rageInvested"] = result.RageInvested.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["findings"] = string.Join("; ", result.Findings)
            });
    }

    private static RuleSetOperationResult ExecuteApplyCombatCondition(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("currentCombatState", out var combatStateText) ||
            !request.Inputs.TryGetValue("conditionKind", out var conditionKindText) ||
            !Enum.TryParse<WerewolfCombatConditionKind>(conditionKindText, true, out var conditionKind))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidConditionRequest", "Condition application requires currentCombatState and conditionKind.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var condition = conditionKind switch
        {
            WerewolfCombatConditionKind.Blinded => new WerewolfCombatCondition(WerewolfCombatConditionKind.Blinded, "Line 3107", "Cannot dodge/parry/block, +2 difficulty to all actions", DifficultyModifier: 2, CanDodge: false, CanParry: false, CanBlock: false),
            WerewolfCombatConditionKind.Immobilized => new WerewolfCombatCondition(WerewolfCombatConditionKind.Immobilized, "Line 3109", "Totally immobile; automatic failure on actions", DifficultyModifier: -2),
            WerewolfCombatConditionKind.Stunned => new WerewolfCombatCondition(WerewolfCombatConditionKind.Stunned, "Line 3111", "No actions except stagger, +2 difficulty to received attacks next turn", CanAct: false),
            WerewolfCombatConditionKind.Prone => new WerewolfCombatCondition(WerewolfCombatConditionKind.Prone, "Lines 3110-3111", "Knocked down; requires actions to stand"),
            WerewolfCombatConditionKind.ChangeAction => new WerewolfCombatCondition(WerewolfCombatConditionKind.ChangeAction, "Line 3108", "+1 difficulty except aborting to defensive", DifficultyModifier: 1),
            _ => null
        };

        if (condition is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidConditionKind", $"Unknown condition kind: {conditionKind}")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var combatState = System.Text.Json.JsonSerializer.Deserialize<WerewolfCombatState>(combatStateText, JsonOptions);
        if (combatState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidCombatState", "Combat state is not valid.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var updatedCombatState = WerewolfCombatStateService.AddCondition(combatState, condition);

        return new RuleSetOperationResult(
            true,
            null,
            [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "ConditionApplied", $"Condition {conditionKind} applied.")],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["updatedCombatState"] = System.Text.Json.JsonSerializer.Serialize(updatedCombatState, JsonOptions),
                ["conditionKind"] = conditionKind.ToString(),
                ["difficultyModifier"] = condition.DifficultyModifier.HasValue ? condition.DifficultyModifier.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : string.Empty
            });
    }

    private static RuleSetOperationResult ExecuteTransitionCombatState(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("currentCombatState", out var combatStateText) ||
            !request.Inputs.TryGetValue("expectedCombatStateVersion", out var versionText) ||
            !int.TryParse(versionText, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidTransitionRequest", "Transition requires currentCombatState and expectedCombatStateVersion.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var combatState = System.Text.Json.JsonSerializer.Deserialize<WerewolfCombatState>(combatStateText, JsonOptions);
        if (combatState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidCombatState", "Combat state is not valid.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        if (combatState.CombatStateVersion != expectedVersion)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "StaleVersion", $"Version mismatch: expected {expectedVersion}, actual {combatState.CombatStateVersion}")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var updatedCombatState = combatState with { CombatStateVersion = combatState.CombatStateVersion + 1 };

        return new RuleSetOperationResult(
            true,
            null,
            [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "CombatStateTransitioned", "Combat state transitioned.")],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["updatedCombatState"] = System.Text.Json.JsonSerializer.Serialize(updatedCombatState, JsonOptions),
                ["newCombatStateVersion"] = updatedCombatState.CombatStateVersion.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
    }

    private static RuleSetOperationResult ExecuteResolveActionResolution(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("currentState", out var currentStateText) ||
            !request.Inputs.TryGetValue("expectedRuntimeStateVersion", out var expectedVersionText) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
            !request.Inputs.TryGetValue("requestId", out var requestId) ||
            !request.Inputs.TryGetValue("attributeId", out var attributeId) ||
            !request.Inputs.TryGetValue("abilityId", out var abilityId) ||
            !request.Inputs.TryGetValue("baseDifficulty", out var baseDifficultyText) ||
            !int.TryParse(baseDifficultyText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var baseDifficulty))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidActionResolutionRequest", "Action resolution requires currentState, expectedRuntimeStateVersion, requestId, attributeId, abilityId, and baseDifficulty.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentState = System.Text.Json.JsonSerializer.Deserialize<WerewolfRuntimeCharacterState>(currentStateText, JsonOptions);
        if (currentState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidCurrentState", "Current state is not valid.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var isDaylight = request.Inputs.TryGetValue("isDaylightWithoutProtection", out var daylightText) &&
            bool.TryParse(daylightText, out var daylight) && daylight;
        var isTension = request.Inputs.TryGetValue("isUnderTension", out var tensionText) &&
            bool.TryParse(tensionText, out var tension) && tension;
        var isWitheredLimb = request.Inputs.TryGetValue("isUsingWitheredLimb", out var limbText) &&
            bool.TryParse(limbText, out var limb) && limb;
        var senseTested = request.Inputs.TryGetValue("senseBeingTested", out var senseText) ? senseText : null;
        var isTracking = request.Inputs.TryGetValue("isTracking", out var trackingText) &&
            bool.TryParse(trackingText, out var tracking) && tracking;
        var isVision = request.Inputs.TryGetValue("isVisionBased", out var visionText) &&
            bool.TryParse(visionText, out var vision) && vision;
        var isBalance = request.Inputs.TryGetValue("isBalanceTest", out var balanceText) &&
            bool.TryParse(balanceText, out var balance) && balance;

        var resolutionRequest = new WerewolfActionResolutionRequest(
            requestId,
            currentState,
            expectedVersion,
            attributeId,
            abilityId,
            baseDifficulty,
            isDaylight,
            isTension,
            isWitheredLimb,
            senseTested,
            isTracking,
            isVision,
            isBalance);

        var result = WerewolfActionResolutionService.ResolveActionTest(resolutionRequest);

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "ActionResolution", f)).ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = result.RequestId,
                ["basePool"] = result.BasePool.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["dicePoolModifier"] = result.DicePoolModifier.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["finalPool"] = result.FinalPool.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["baseDifficulty"] = result.BaseDifficulty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["difficultyModifier"] = result.DifficultyModifier.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["finalDifficulty"] = result.FinalDifficulty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["isActionUnavailable"] = result.IsActionUnavailable.ToString(),
                ["isAutomaticFailure"] = result.IsAutomaticFailure.ToString(),
                ["conditionalTests"] = System.Text.Json.JsonSerializer.Serialize(result.ConditionalTests.Select(c => new { c.Condition, c.Target, c.TestDifficulty, c.MinimumSuccesses, c.Consequence }), JsonOptions),
                ["findings"] = string.Join("; ", result.Findings)
            });
    }

    private static RuleSetOperationResult ExecuteApplyCondition(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("currentState", out var currentStateText) ||
            !request.Inputs.TryGetValue("expectedRuntimeStateVersion", out var expectedVersionText) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
            !request.Inputs.TryGetValue("requestId", out var requestId) ||
            !request.Inputs.TryGetValue("conditionKey", out var conditionKey) ||
            !request.Inputs.TryGetValue("conditionKind", out var conditionKindText) ||
            !Enum.TryParse<WerewolfConditionKind>(conditionKindText, true, out var conditionKind) ||
            !request.Inputs.TryGetValue("sourceLocator", out var sourceLocator) ||
            !request.Inputs.TryGetValue("sourceDeformity", out var sourceDeformity))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidApplyConditionRequest", "Apply condition requires currentState, expectedRuntimeStateVersion, requestId, conditionKey, conditionKind, sourceLocator, and sourceDeformity.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentState = System.Text.Json.JsonSerializer.Deserialize<WerewolfRuntimeCharacterState>(currentStateText, JsonOptions);
        if (currentState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidCurrentState", "Current state is not valid.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        int? durationTurns = null;
        if (request.Inputs.TryGetValue("durationTurns", out var durationText) && int.TryParse(durationText, out var duration))
        {
            durationTurns = duration;
        }

        var applyRequest = new WerewolfApplyConditionRequest(
            requestId,
            currentState,
            expectedVersion,
            conditionKey,
            conditionKind,
            sourceLocator,
            sourceDeformity,
            durationTurns);

        var result = WerewolfConditionService.ApplyCondition(applyRequest);

        if (!result.Succeeded || result.NewState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "ApplyConditionFailed", f)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "ConditionApplied", f)).ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["newState"] = System.Text.Json.JsonSerializer.Serialize(result.NewState, JsonOptions),
                ["newRuntimeStateVersion"] = result.NewRuntimeStateVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["findings"] = string.Join("; ", result.Findings)
            });
    }

    private static RuleSetOperationResult ExecuteClearCondition(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("currentState", out var currentStateText) ||
            !request.Inputs.TryGetValue("expectedRuntimeStateVersion", out var expectedVersionText) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
            !request.Inputs.TryGetValue("requestId", out var requestId) ||
            !request.Inputs.TryGetValue("conditionKey", out var conditionKey))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidClearConditionRequest", "Clear condition requires currentState, expectedRuntimeStateVersion, requestId, and conditionKey.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentState = System.Text.Json.JsonSerializer.Deserialize<WerewolfRuntimeCharacterState>(currentStateText, JsonOptions);
        if (currentState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidCurrentState", "Current state is not valid.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var clearRequest = new WerewolfClearConditionRequest(
            requestId,
            currentState,
            expectedVersion,
            conditionKey);

        var result = WerewolfConditionService.ClearCondition(clearRequest);

        if (!result.Succeeded || result.NewState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "ClearConditionFailed", f)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "ConditionCleared", f)).ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["newState"] = System.Text.Json.JsonSerializer.Serialize(result.NewState, JsonOptions),
                ["newRuntimeStateVersion"] = result.NewRuntimeStateVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["findings"] = string.Join("; ", result.Findings)
            });
    }

    private static RuleSetOperationResult ExecuteEvaluateActionAvailability(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("currentState", out var currentStateText) ||
            !request.Inputs.TryGetValue("expectedRuntimeStateVersion", out var expectedVersionText) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
            !request.Inputs.TryGetValue("requestId", out var requestId) ||
            !request.Inputs.TryGetValue("actionType", out var actionType))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidAvailabilityRequest", "Availability evaluation requires currentState, expectedRuntimeStateVersion, requestId, and actionType.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentState = System.Text.Json.JsonSerializer.Deserialize<WerewolfRuntimeCharacterState>(currentStateText, JsonOptions);
        if (currentState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidCurrentState", "Current state is not valid.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var availabilityRequest = new WerewolfEvaluateActionAvailabilityRequest(
            requestId,
            currentState,
            expectedVersion,
            actionType);

        var result = WerewolfConditionService.EvaluateActionAvailability(availabilityRequest);

        return new RuleSetOperationResult(
            result.Succeeded,
            null,
            result.Findings.Select(f => new RuleSetRuntimeFinding(
                result.IsAvailable ? RuleSetRuntimeFindingSeverity.Information : RuleSetRuntimeFindingSeverity.Error,
                "ActionAvailability",
                f)).ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                 ["isAvailable"] = result.IsAvailable.ToString(),
                ["unavailableReason"] = result.UnavailableReason ?? string.Empty,
                ["findings"] = string.Join("; ", result.Findings)
            });
    }

    private static RuleSetOperationResult ExecuteDefineManeuver(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("maneuverId", out var maneuverId))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidManeuverRequest", "Maneuver definition requires maneuverId.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var maneuver = WerewolfCombatManeuverCatalog.Entries.FirstOrDefault(m => StringComparer.Ordinal.Equals(m.ManeuverId, maneuverId));
        if (maneuver is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidManeuverId", $"Unknown maneuver: {maneuverId}")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "ManeuverDefined", $"Maneuver {maneuverId} defined.")],
              new Dictionary<string, string>(StringComparer.Ordinal)
              {
                  ["maneuverId"] = maneuverId,
                  ["sourceLocator"] = maneuver.SourceLocator,
                  ["allowedForms"] = string.Join(",", maneuver.AllowedForms),
                  ["baseDifficulty"] = maneuver.BaseDifficulty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                  ["damageCategory"] = maneuver.DamageCategory ?? string.Empty,
                  ["damageExpression"] = maneuver.DamageExpression ?? string.Empty,
                  ["actionCost"] = maneuver.ActionCost.ToString(System.Globalization.CultureInfo.InvariantCulture),
                  ["notes"] = maneuver.Notes ?? string.Empty
              });
     }

     private static RuleSetOperationResult ExecuteFrenzyDefineTest(RuleSetOperationRequest request)
     {
         if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
             !request.Inputs.TryGetValue("ragePermanent", out var rageText) ||
             !request.Inputs.TryGetValue("willpowerPermanent", out var willpowerText) ||
             !request.Inputs.TryGetValue("rank", out var rankText) ||
             !int.TryParse(rageText, out var ragePermanent) ||
             !int.TryParse(willpowerText, out var willpowerPermanent) ||
             !int.TryParse(rankText, out var rank))
         {
             return new RuleSetOperationResult(
                 false,
                 RuleSetOperationFailureCode.InvalidRequest,
                 [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidFrenzyTestDefinitionRequest", "Frenzy test definition requires requestId, ragePermanent, willpowerPermanent, and rank.")],
                 new Dictionary<string, string>(StringComparer.Ordinal));
         }

         var currentForm = request.Inputs.GetValueOrDefault("currentForm");
         var auspiceMoon = request.Inputs.GetValueOrDefault("auspiceMoon");
         var moonPhase = request.Inputs.GetValueOrDefault("moonPhase");
         var environmentalModifier = request.Inputs.GetValueOrDefault("environmentalModifier");

          var result = WerewolfFrenzyTestDefinitionService.ComputeTestDefinition(
              requestId,
              ragePermanent,
              willpowerPermanent,
              rank,
              currentForm,
              auspiceMoon,
              moonPhase,
              environmentalModifier);

         if (!result.IsValid)
         {
             return new RuleSetOperationResult(
                 false,
                 RuleSetOperationFailureCode.InvalidRequest,
                 result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidFrenzyTestDefinition", f)).ToArray(),
                 new Dictionary<string, string>(StringComparer.Ordinal));
         }

         return new RuleSetOperationResult(
             true,
             null,
             result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "FrenzyTestDefined", f)).ToArray(),
             new Dictionary<string, string>(StringComparer.Ordinal)
             {
                 ["requestId"] = result.RequestId,
                 ["dicePool"] = result.DicePool.ToString(System.Globalization.CultureInfo.InvariantCulture),
                 ["baseDifficulty"] = result.BaseDifficulty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                 ["finalDifficulty"] = result.FinalDifficulty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                 ["difficultyModifier"] = result.DifficultyModifier.ToString(System.Globalization.CultureInfo.InvariantCulture),
                 ["successThreshold"] = result.SuccessThreshold.ToString(System.Globalization.CultureInfo.InvariantCulture)
             });
     }

     private static RuleSetOperationResult ExecuteFrenzyEnter(RuleSetOperationRequest request)
     {
         var currentState = GetCurrentRuntimeState(request);
         var expectedVersion = GetExpectedVersion(request);
         var requestId = request.Inputs.GetValueOrDefault("requestId", string.Empty);
         var frenzyTypeText = request.Inputs.GetValueOrDefault("frenzyType", string.Empty);
         var trigger = request.Inputs.GetValueOrDefault("trigger", string.Empty);
         var successesText = request.Inputs.GetValueOrDefault("accumulatedSuccesses", "0");
         var targetRestriction = request.Inputs.GetValueOrDefault("targetRestriction");

         if (!Enum.TryParse<WerewolfFrenzyType>(frenzyTypeText, true, out var frenzyType))
         {
             return new RuleSetOperationResult(
                 false,
                 RuleSetOperationFailureCode.InvalidRequest,
                 [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidFrenzyType", $"Invalid frenzy type: {frenzyTypeText}")],
                 new Dictionary<string, string>(StringComparer.Ordinal));
         }

         if (!int.TryParse(successesText, out var accumulatedSuccesses))
         {
             accumulatedSuccesses = 0;
         }

         var result = WerewolfFrenzyResolutionService.EnterFrenzy(new WerewolfEnterFrenzyRequest(
             requestId, currentState, expectedVersion, frenzyType, trigger, accumulatedSuccesses, targetRestriction));

         if (!result.Succeeded || result.UpdatedState is null)
         {
             return new RuleSetOperationResult(
                 false,
                 RuleSetOperationFailureCode.InvalidRequest,
                 result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, result.ErrorCode ?? "EnterFrenzyFailed", f)).ToArray(),
                 new Dictionary<string, string>(StringComparer.Ordinal));
         }

         return new RuleSetOperationResult(
             true,
             null,
             result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "FrenzyEntered", f)).ToArray(),
             new Dictionary<string, string>(StringComparer.Ordinal)
             {
                 ["newState"] = System.Text.Json.JsonSerializer.Serialize(result.UpdatedState, JsonOptions),
                 ["newRuntimeStateVersion"] = result.UpdatedState.RuntimeStateVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                 ["frenzyType"] = frenzyType.ToString(),
                 ["findings"] = string.Join("; ", result.Findings)
             });
     }

     private static RuleSetOperationResult ExecuteFrenzySuppress(RuleSetOperationRequest request)
     {
         var currentState = GetCurrentRuntimeState(request);
         var expectedVersion = GetExpectedVersion(request);

         var result = WerewolfFrenzyResolutionService.SuppressFrenzy(currentState, expectedVersion);

         if (!result.Succeeded || result.UpdatedState is null)
         {
             return new RuleSetOperationResult(
                 false,
                 RuleSetOperationFailureCode.InvalidRequest,
                 result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, result.ErrorCode ?? "SuppressFrenzyFailed", f)).ToArray(),
                 new Dictionary<string, string>(StringComparer.Ordinal));
         }

         return new RuleSetOperationResult(
             true,
             null,
             result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "FrenzySuppressed", f)).ToArray(),
             new Dictionary<string, string>(StringComparer.Ordinal)
             {
                 ["newState"] = System.Text.Json.JsonSerializer.Serialize(result.UpdatedState, JsonOptions),
                 ["newRuntimeStateVersion"] = result.UpdatedState.RuntimeStateVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                 ["willpowerCurrent"] = result.UpdatedState.WillpowerCurrent.ToString(System.Globalization.CultureInfo.InvariantCulture),
                 ["findings"] = string.Join("; ", result.Findings)
             });
     }

     private static RuleSetOperationResult ExecuteFrenzyEnd(RuleSetOperationRequest request)
     {
         var currentState = GetCurrentRuntimeState(request);
         var expectedVersion = GetExpectedVersion(request);

         var result = WerewolfFrenzyResolutionService.EndFrenzy(currentState, expectedVersion);

         if (!result.Succeeded || result.UpdatedState is null)
         {
             return new RuleSetOperationResult(
                 false,
                 RuleSetOperationFailureCode.InvalidRequest,
                 result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, result.ErrorCode ?? "EndFrenzyFailed", f)).ToArray(),
                 new Dictionary<string, string>(StringComparer.Ordinal));
         }

         return new RuleSetOperationResult(
             true,
             null,
             result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "FrenzyEnded", f)).ToArray(),
             new Dictionary<string, string>(StringComparer.Ordinal)
             {
                 ["newState"] = System.Text.Json.JsonSerializer.Serialize(result.UpdatedState, JsonOptions),
                 ["newRuntimeStateVersion"] = result.UpdatedState.RuntimeStateVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                 ["findings"] = string.Join("; ", result.Findings)
             });
     }

     private static RuleSetOperationResult ExecuteFrenzyEvaluateAction(RuleSetOperationRequest request)
     {
         var currentState = GetCurrentRuntimeState(request);
         var actionType = request.Inputs.GetValueOrDefault("actionType", string.Empty);

         var availability = WerewolfFrenzyResolutionService.EvaluateFrenzyAction(currentState, actionType);

         return new RuleSetOperationResult(
             true,
             null,
             [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "FrenzyActionEvaluated", $"Action '{actionType}' availability: {availability}")],
             new Dictionary<string, string>(StringComparer.Ordinal)
             {
                 ["actionType"] = actionType,
                 ["availability"] = availability,
                  ["isAvailable"] = availability == "available" ? "true" : "false"
              });
      }

      private static RuleSetOperationResult ExecuteActivateGift(RuleSetOperationRequest request)
      {
          var currentState = GetCurrentRuntimeState(request);
          var expectedVersion = GetExpectedVersion(request);
          var giftKey = request.Inputs.GetValueOrDefault("giftKey", string.Empty);

          var result = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
              request.Inputs.GetValueOrDefault("requestId", string.Empty),
              currentState,
              expectedVersion,
              giftKey));

          if (!result.Succeeded || result.UpdatedState is null || result.ActivationDefinition is null)
          {
              return new RuleSetOperationResult(
                  false,
                  RuleSetOperationFailureCode.InvalidRequest,
                  result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, result.ErrorCode ?? "GiftActivationFailed", f)).ToArray(),
                  new Dictionary<string, string>(StringComparer.Ordinal));
          }

          return new RuleSetOperationResult(
              true,
              null,
              result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "GiftActivated", f)).ToArray(),
              new Dictionary<string, string>(StringComparer.Ordinal)
              {
                  ["giftKey"] = result.ActivationDefinition.GiftKey,
                  ["giftName"] = result.ActivationDefinition.GiftName,
                  ["dicePool"] = result.ActivationDefinition.DicePool.ToString(System.Globalization.CultureInfo.InvariantCulture),
                  ["difficulty"] = result.ActivationDefinition.Difficulty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                  ["testComponents"] = string.Join(",", result.ActivationDefinition.TestComponents),
                  ["costType"] = result.ActivationDefinition.CostType.ToString(),
                  ["costAmount"] = result.ActivationDefinition.CostAmount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                  ["costPaid"] = result.ActivationDefinition.CostPaid.ToString(),
                  ["durationType"] = result.ActivationDefinition.DurationType.ToString(),
                  ["durationTurns"] = result.ActivationDefinition.DurationTurns.ToString(System.Globalization.CultureInfo.InvariantCulture),
                  ["sourceLocator"] = result.ActivationDefinition.SourceLocator,
                  ["newState"] = System.Text.Json.JsonSerializer.Serialize(result.UpdatedState, JsonOptions),
                  ["newRuntimeStateVersion"] = result.UpdatedState.RuntimeStateVersion.ToString(System.Globalization.CultureInfo.InvariantCulture)
              });
      }

      private static RuleSetOperationResult ExecuteGiftEffect(RuleSetOperationRequest request)
      {
          var currentState = GetCurrentRuntimeState(request);
          var expectedVersion = GetExpectedVersion(request);
          var giftKey = request.Inputs.GetValueOrDefault("giftKey", string.Empty);
          var successesText = request.Inputs.GetValueOrDefault("activationSuccesses", "0");
          if (!int.TryParse(successesText, out var activationSuccesses))
          {
              activationSuccesses = 0;
          }

          IReadOnlyList<int>? diceValues = null;
          var diceValuesText = request.Inputs.GetValueOrDefault("diceValues", string.Empty);
          if (!string.IsNullOrWhiteSpace(diceValuesText))
          {
              var parsed = new List<int>();
              foreach (var part in diceValuesText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
              {
                  if (int.TryParse(part, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var value))
                  {
                      parsed.Add(value);
                  }
              }

              if (parsed.Count > 0)
              {
                  diceValues = Array.AsReadOnly(parsed.ToArray());
              }
          }

          var result = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
              request.Inputs.GetValueOrDefault("requestId", string.Empty),
              currentState,
              expectedVersion,
              giftKey,
              activationSuccesses,
              request.Inputs.GetValueOrDefault("targetId", string.Empty),
              diceValues));

          if (!result.Succeeded || result.UpdatedState is null)
          {
              return new RuleSetOperationResult(
                  false,
                  RuleSetOperationFailureCode.InvalidRequest,
                  result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, result.ErrorCode ?? "GiftEffectFailed", f)).ToArray(),
                  new Dictionary<string, string>(StringComparer.Ordinal));
          }

          var outputs = new Dictionary<string, string>(StringComparer.Ordinal)
          {
              ["giftKey"] = giftKey,
              ["activationSuccesses"] = activationSuccesses.ToString(System.Globalization.CultureInfo.InvariantCulture),
              ["newState"] = System.Text.Json.JsonSerializer.Serialize(result.UpdatedState, JsonOptions),
              ["newRuntimeStateVersion"] = result.UpdatedState.RuntimeStateVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
              ["activeEffectsCount"] = result.ActiveEffects.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
              ["findings"] = string.Join("; ", result.Findings)
          };

          if (result.Payload is not null)
          {
              outputs["payload"] = System.Text.Json.JsonSerializer.Serialize(result.Payload, JsonOptions);
          }

          for (var i = 0; i < result.ActiveEffects.Count; i++)
          {
              var effect = result.ActiveEffects[i];
              outputs[$"activeEffect_{i}_giftKey"] = effect.GiftKey;
              outputs[$"activeEffect_{i}_kind"] = effect.EffectKind.ToString();
              outputs[$"activeEffect_{i}_magnitude"] = effect.Magnitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
              outputs[$"activeEffect_{i}_durationTurns"] = effect.RemainingDuration.ToString(System.Globalization.CultureInfo.InvariantCulture);
              outputs[$"activeEffect_{i}_sourceLocator"] = effect.SourceLocator;
          }

           return new RuleSetOperationResult(
               true,
               null,
               result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "GiftEffectApplied", f)).ToArray(),
               outputs);
      }

       private static RuleSetOperationResult ExecuteCalculateAdvancementCost(RuleSetOperationRequest request)
       {
           var currentState = GetCurrentRuntimeState(request);
           var expectedVersion = GetExpectedVersion(request);
           var traitType = request.Inputs.GetValueOrDefault("traitType", string.Empty);
           var traitIdentifier = request.Inputs.GetValueOrDefault("traitIdentifier", string.Empty);
           var currentRatingText = request.Inputs.GetValueOrDefault("currentRating", "0");
           if (!int.TryParse(currentRatingText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var currentRating))
           {
               currentRating = 0;
           }

           var result = WerewolfAdvancementCostService.CalculateCost(new WerewolfAdvancementCostRequest(currentState, expectedVersion, traitType, traitIdentifier, currentRating));

           if (!result.Succeeded || result.Cost is null)
           {
               return new RuleSetOperationResult(
                   false,
                   RuleSetOperationFailureCode.InvalidRequest,
                   result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                   new Dictionary<string, string>(StringComparer.Ordinal));
           }

           return new RuleSetOperationResult(
               true,
               null,
               result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, finding.Code.ToString(), finding.Message)).ToArray(),
               new Dictionary<string, string>(StringComparer.Ordinal)
               {
                   ["cost"] = result.Cost.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
                   ["traitType"] = traitType,
                   ["traitIdentifier"] = traitIdentifier ?? string.Empty,
                   ["currentRating"] = currentRating.ToString(System.Globalization.CultureInfo.InvariantCulture)
               });
       }

       private static RuleSetOperationResult ExecuteAdvanceTrait(RuleSetOperationRequest request)
       {
           var currentState = GetCurrentRuntimeState(request);
           var expectedVersion = GetExpectedVersion(request);
           var requestId = request.Inputs.GetValueOrDefault("requestId", string.Empty);
           var traitType = request.Inputs.GetValueOrDefault("traitType", string.Empty);
           var traitIdentifier = request.Inputs.GetValueOrDefault("traitIdentifier", string.Empty);

           var result = WerewolfAdvancementService.Advance(new WerewolfAdvanceTraitRequest(currentState, expectedVersion, requestId, traitType, traitIdentifier));

           if (!result.Succeeded || result.NewState is null)
           {
               return new RuleSetOperationResult(
                   false,
                   RuleSetOperationFailureCode.InvalidRequest,
                   result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                   new Dictionary<string, string>(StringComparer.Ordinal)
                   {
                       ["remainingXp"] = result.RemainingXp?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
                   });
           }

           return new RuleSetOperationResult(
               true,
               null,
               result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, finding.Code.ToString(), finding.Message)).ToArray(),
               new Dictionary<string, string>(StringComparer.Ordinal)
               {
                   ["requestId"] = result.RequestId ?? string.Empty,
                   ["newState"] = System.Text.Json.JsonSerializer.Serialize(result.NewState, JsonOptions),
                   ["newRuntimeStateVersion"] = result.NewRuntimeStateVersion?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                   ["xpSpent"] = result.XpSpent?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                   ["remainingXp"] = result.RemainingXp?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
               });
       }

       private static RuleSetOperationResult ExecuteEvaluateSpecialtyEligibility(RuleSetOperationRequest request)
       {
           var traitType = request.Inputs.GetValueOrDefault("traitType", string.Empty);
           var traitIdentifier = request.Inputs.GetValueOrDefault("traitIdentifier", string.Empty);
           var currentRatingText = request.Inputs.GetValueOrDefault("currentRating", "0");
           if (!int.TryParse(currentRatingText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var currentRating))
           {
               currentRating = 0;
           }

           var result = WerewolfSpecialtyEligibilityService.Evaluate(new WerewolfSpecialtyEligibilityRequest(traitType, traitIdentifier, currentRating));

           return new RuleSetOperationResult(
               result.Succeeded,
               result.Succeeded ? null : RuleSetOperationFailureCode.InvalidRequest,
               result.Findings.Select(finding => new RuleSetRuntimeFinding(
                       finding.Severity == WerewolfProgressionFindingSeverity.Error ? RuleSetRuntimeFindingSeverity.Error : RuleSetRuntimeFindingSeverity.Information,
                   finding.Code.ToString(),
                   finding.Message)).ToArray(),
               new Dictionary<string, string>(StringComparer.Ordinal)
               {
                   ["isEligible"] = result.IsEligible.ToString(System.Globalization.CultureInfo.InvariantCulture),
                   ["traitType"] = traitType,
                   ["traitIdentifier"] = traitIdentifier,
                   ["currentRating"] = currentRating.ToString(System.Globalization.CultureInfo.InvariantCulture)
               });
       }

        private static RuleSetOperationResult ExecuteEvaluateGiftAdvancement(RuleSetOperationRequest request)
        {
            var currentState = GetCurrentRuntimeState(request);
            var expectedVersion = GetExpectedVersion(request);
            var giftKey = request.Inputs.GetValueOrDefault("giftKey", string.Empty);

            var result = WerewolfGiftAdvancementEligibilityService.Evaluate(new WerewolfGiftAdvancementRequest(currentState, expectedVersion, giftKey));

            if (!result.Succeeded)
            {
                return new RuleSetOperationResult(
                    false,
                    RuleSetOperationFailureCode.InvalidRequest,
                    result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                    new Dictionary<string, string>(StringComparer.Ordinal));
            }

            return new RuleSetOperationResult(
                true,
                null,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(
                        finding.Severity == WerewolfProgressionFindingSeverity.Error ? RuleSetRuntimeFindingSeverity.Error : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code.ToString(),
                    finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["giftKey"] = giftKey,
                    ["cost"] = result.Cost?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                    ["isEligible"] = result.IsEligible?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                    ["ineligibilityReason"] = result.IneligibilityReason ?? string.Empty
                });
        }

        private static RuleSetOperationResult ExecuteRite(RuleSetOperationRequest request)
        {
            if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
                !request.Inputs.TryGetValue("riteKey", out var riteKey) ||
                !request.Inputs.TryGetValue("diceValues", out var diceValuesText))
            {
                return new RuleSetOperationResult(
                    false,
                    RuleSetOperationFailureCode.InvalidRequest,
                    [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidRiteExecutionRequest", "Rite execution requires requestId, riteKey, and diceValues.")],
                    new Dictionary<string, string>(StringComparer.Ordinal));
            }

            var hasTargetPieceText = request.Inputs.GetValueOrDefault("hasTargetPiece", "false");
            var hasTargetPiece = bool.TryParse(hasTargetPieceText, out var parsedHasTargetPiece) && parsedHasTargetPiece;

            var diceValues = ParseCsv(diceValuesText)
                .Select(value => int.TryParse(value, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var die) ? die : 0)
                .ToArray();

            var executionRequest = new WerewolfRiteExecutionRequest(
                requestId,
                riteKey,
                Array.AsReadOnly(diceValues),
                hasTargetPiece);

            var result = WerewolfRiteExecutionService.Execute(executionRequest);

            if (!result.Succeeded)
            {
                return new RuleSetOperationResult(
                    false,
                    RuleSetOperationFailureCode.InvalidRequest,
                    result.Findings.Select(finding => new RuleSetRuntimeFinding(
                            finding.Severity == WerewolfRiteFindingSeverity.Error
                                ? RuleSetRuntimeFindingSeverity.Error
                                : RuleSetRuntimeFindingSeverity.Information,
                        finding.Code,
                        finding.Message)).ToArray(),
                    new Dictionary<string, string>(StringComparer.Ordinal));
            }

            return new RuleSetOperationResult(
                true,
                null,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(
                        finding.Severity == WerewolfRiteFindingSeverity.Error
                            ? RuleSetRuntimeFindingSeverity.Error
                            : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code,
                    finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["requestId"] = result.RequestId,
                    ["riteKey"] = result.RiteKey,
                    ["dicePool"] = result.DicePool.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["difficulty"] = result.Difficulty.HasValue ? result.Difficulty.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : string.Empty,
                    ["successCount"] = result.SuccessCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["interpretationStatus"] = result.InterpretationStatus,
                    ["effect"] = result.Effect ?? string.Empty
                });
        }

        private static RuleSetOperationResult ExecuteInitializeSpirit(RuleSetOperationRequest request)
        {
            if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
                !request.Inputs.TryGetValue("spiritId", out var spiritId) ||
                !request.Inputs.TryGetValue("categoryKey", out var categoryKey) ||
                !request.Inputs.TryGetValue("willpowerPermanent", out var willpowerText) ||
                !request.Inputs.TryGetValue("ragePermanent", out var rageText) ||
                !request.Inputs.TryGetValue("gnosisPermanent", out var gnosisText) ||
                !int.TryParse(willpowerText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var willpowerPermanent) ||
                !int.TryParse(rageText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var ragePermanent) ||
                !int.TryParse(gnosisText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var gnosisPermanent))
            {
                return InvalidSpiritRequest("Initialize spirit requires requestId, spiritId, categoryKey, willpowerPermanent, ragePermanent, gnosisPermanent.");
            }

            var charmKeys = ParseCsv(request.Inputs.GetValueOrDefault("knownCharmKeys"));
            var spiritRequest = new SpiritMechanicRequest(null!, 0, requestId);
            var result = WerewolfSpiritMechanicServices.Initialize(spiritRequest, spiritId, categoryKey, willpowerPermanent, ragePermanent, gnosisPermanent, charmKeys);

            return new RuleSetOperationResult(
                result.Succeeded,
                result.Succeeded ? null : RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(f => new RuleSetRuntimeFinding(f.Severity == SpiritMechanicFindingSeverity.Error ? RuleSetRuntimeFindingSeverity.Error : RuleSetRuntimeFindingSeverity.Information, f.Code.ToString(), f.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["succeeded"] = result.Succeeded.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["spiritState"] = result.NewState is null ? string.Empty : System.Text.Json.JsonSerializer.Serialize(result.NewState, JsonOptions),
                    ["stateVersion"] = result.NewStateVersion?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
                });
        }

        private static RuleSetOperationResult ExecuteEvaluateCrossing(RuleSetOperationRequest request)
        {
            if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
                !request.Inputs.TryGetValue("currentState", out var stateJson) ||
                !request.Inputs.TryGetValue("expectedStateVersion", out var versionText) ||
                !request.Inputs.TryGetValue("gauntletValue", out var gauntletText) ||
                !request.Inputs.TryGetValue("gnosisPool", out var gnosisText) ||
                !request.Inputs.TryGetValue("difficulty", out var difficultyText) ||
                !request.Inputs.TryGetValue("diceValues", out var diceText) ||
                !int.TryParse(versionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
                !int.TryParse(gauntletText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var gauntletValue) ||
                !int.TryParse(gnosisText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var gnosisPool) ||
                !int.TryParse(difficultyText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var difficulty))
            {
                return InvalidSpiritRequest("Crossing evaluation requires requestId, currentState, expectedStateVersion, gauntletValue, gnosisPool, difficulty, diceValues.");
            }

            var state = System.Text.Json.JsonSerializer.Deserialize<WerewolfSpiritRuntimeState>(stateJson, JsonOptions);
            if (state is null)
            {
                return InvalidSpiritRequest("Failed to deserialize spirit state.");
            }

            var diceValues = ParseCsv(diceText).Select(v => int.TryParse(v, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0).ToArray();
            var hasReflectiveSurface = bool.TryParse(request.Inputs.GetValueOrDefault("hasReflectiveSurface"), out var r) && r;
            var silverCount = int.TryParse(request.Inputs.GetValueOrDefault("silverItemCount"), System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var s) ? s : 0;
            var isFuryGranted = bool.TryParse(request.Inputs.GetValueOrDefault("isFuryGrantedAction"), out var f) && f;
            var previousAttempts = int.TryParse(request.Inputs.GetValueOrDefault("previousFailedAttempts"), System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var p) ? p : 0;

            var crossingRequest = new CrossingRequest(state, expectedVersion, requestId, gauntletValue, gnosisPool, difficulty, hasReflectiveSurface, silverCount, isFuryGranted, previousAttempts, diceValues);
            var result = WerewolfSpiritMechanicServices.EvaluateCrossing(crossingRequest);

            return new RuleSetOperationResult(
                result.Succeeded,
                result.Succeeded ? null : RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(f => new RuleSetRuntimeFinding(f.Severity == SpiritMechanicFindingSeverity.Error ? RuleSetRuntimeFindingSeverity.Error : RuleSetRuntimeFindingSeverity.Information, f.Code.ToString(), f.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["succeeded"] = result.Succeeded.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["successes"] = result.Successes.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["isBotch"] = result.IsBotch.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["isZeroSuccessWait"] = result.IsZeroSuccessWait.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["isFuryRestricted"] = result.IsFuryRestricted.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["crossingTime"] = result.CrossingTime.ToString(),
                    ["effectiveGnosis"] = result.EffectiveGnosis.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["effectiveDifficulty"] = result.EffectiveDifficulty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["canRetry"] = result.CanRetry.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["nextRetryDifficultyModifier"] = result.NextRetryDifficultyModifier.ToString(System.Globalization.CultureInfo.InvariantCulture)
                });
        }

        private static RuleSetOperationResult ExecuteComputeMovementSpeed(RuleSetOperationRequest request)
        {
            if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
                !request.Inputs.TryGetValue("currentState", out var stateJson) ||
                !request.Inputs.TryGetValue("expectedStateVersion", out var versionText) ||
                !int.TryParse(versionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
            {
                return InvalidSpiritRequest("Movement speed requires requestId, currentState, expectedStateVersion.");
            }

            var state = System.Text.Json.JsonSerializer.Deserialize<WerewolfSpiritRuntimeState>(stateJson, JsonOptions);
            if (state is null)
            {
                return InvalidSpiritRequest("Failed to deserialize spirit state.");
            }

            var movementRequest = new MovementRequest(state, expectedVersion, requestId);
            var result = WerewolfSpiritMechanicServices.ComputeMovementSpeed(movementRequest);

            return new RuleSetOperationResult(
                result.Succeeded,
                result.Succeeded ? null : RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(f => new RuleSetRuntimeFinding(f.Severity == SpiritMechanicFindingSeverity.Error ? RuleSetRuntimeFindingSeverity.Error : RuleSetRuntimeFindingSeverity.Information, f.Code.ToString(), f.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["succeeded"] = result.Succeeded.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["maxMetersPerTurn"] = result.MaxMetersPerTurn.ToString(System.Globalization.CultureInfo.InvariantCulture)
                });
        }

        private static RuleSetOperationResult ExecuteEvaluateDetection(RuleSetOperationRequest request)
        {
            if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
                !request.Inputs.TryGetValue("currentState", out var stateJson) ||
                !request.Inputs.TryGetValue("expectedStateVersion", out var versionText) ||
                !request.Inputs.TryGetValue("gauntletValue", out var gauntletText) ||
                !request.Inputs.TryGetValue("gnosisPool", out var gnosisText) ||
                !request.Inputs.TryGetValue("difficulty", out var difficultyText) ||
                !request.Inputs.TryGetValue("diceValues", out var diceText) ||
                !int.TryParse(versionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
                !int.TryParse(gauntletText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var gauntletValue) ||
                !int.TryParse(gnosisText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var gnosisPool) ||
                !int.TryParse(difficultyText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var difficulty))
            {
                return InvalidSpiritRequest("Detection requires requestId, currentState, expectedStateVersion, gauntletValue, gnosisPool, difficulty, diceValues.");
            }

            var state = System.Text.Json.JsonSerializer.Deserialize<WerewolfSpiritRuntimeState>(stateJson, JsonOptions);
            if (state is null)
            {
                return InvalidSpiritRequest("Failed to deserialize spirit state.");
            }

            var diceValues = ParseCsv(diceText).Select(v => int.TryParse(v, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0).ToArray();
            var detectionRequest = new DetectionRequest(state, expectedVersion, requestId, gauntletValue, gnosisPool, difficulty, diceValues);
            var result = WerewolfSpiritMechanicServices.EvaluateDetection(detectionRequest);

            return new RuleSetOperationResult(
                result.Succeeded,
                result.Succeeded ? null : RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(f => new RuleSetRuntimeFinding(f.Severity == SpiritMechanicFindingSeverity.Error ? RuleSetRuntimeFindingSeverity.Error : RuleSetRuntimeFindingSeverity.Information, f.Code.ToString(), f.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["succeeded"] = result.Succeeded.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["isAutomatic"] = result.IsAutomatic.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["isDetected"] = result.IsDetected.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["successes"] = result.Successes.ToString(System.Globalization.CultureInfo.InvariantCulture)
                });
        }

        private static RuleSetOperationResult ExecuteEvaluateMaterialization(RuleSetOperationRequest request)
        {
            if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
                !request.Inputs.TryGetValue("currentState", out var stateJson) ||
                !request.Inputs.TryGetValue("expectedStateVersion", out var versionText) ||
                !request.Inputs.TryGetValue("gauntletValue", out var gauntletText) ||
                !int.TryParse(versionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
                !int.TryParse(gauntletText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var gauntletValue))
            {
                return InvalidSpiritRequest("Materialization requires requestId, currentState, expectedStateVersion, gauntletValue.");
            }

            var state = System.Text.Json.JsonSerializer.Deserialize<WerewolfSpiritRuntimeState>(stateJson, JsonOptions);
            if (state is null)
            {
                return InvalidSpiritRequest("Failed to deserialize spirit state.");
            }

            var materializationRequest = new MaterializationRequest(state, expectedVersion, requestId, gauntletValue);
            var result = WerewolfSpiritMechanicServices.EvaluateMaterialization(materializationRequest);

            return new RuleSetOperationResult(
                result.Succeeded,
                result.Succeeded ? null : RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(f => new RuleSetRuntimeFinding(f.Severity == SpiritMechanicFindingSeverity.Error ? RuleSetRuntimeFindingSeverity.Error : RuleSetRuntimeFindingSeverity.Information, f.Code.ToString(), f.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["succeeded"] = result.Succeeded.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["canMaterialize"] = result.CanMaterialize.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["isNowMaterialized"] = result.IsNowMaterialized.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["newState"] = result.NewState is null ? string.Empty : System.Text.Json.JsonSerializer.Serialize(result.NewState, JsonOptions),
                    ["newStateVersion"] = result.NewStateVersion?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
                });
        }

        private static RuleSetOperationResult ExecuteSpendEssence(RuleSetOperationRequest request)
        {
            if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
                !request.Inputs.TryGetValue("currentState", out var stateJson) ||
                !request.Inputs.TryGetValue("expectedStateVersion", out var versionText) ||
                !request.Inputs.TryGetValue("amount", out var amountText) ||
                !int.TryParse(versionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
                !int.TryParse(amountText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var amount))
            {
                return InvalidSpiritRequest("Essence spend requires requestId, currentState, expectedStateVersion, amount.");
            }

            var state = System.Text.Json.JsonSerializer.Deserialize<WerewolfSpiritRuntimeState>(stateJson, JsonOptions);
            if (state is null)
            {
                return InvalidSpiritRequest("Failed to deserialize spirit state.");
            }

            var spendRequest = new EssenceSpendRequest(state, expectedVersion, requestId, amount);
            var result = WerewolfSpiritMechanicServices.SpendEssence(spendRequest);

            return new RuleSetOperationResult(
                result.Succeeded,
                result.Succeeded ? null : RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(f => new RuleSetRuntimeFinding(f.Severity == SpiritMechanicFindingSeverity.Error ? RuleSetRuntimeFindingSeverity.Error : RuleSetRuntimeFindingSeverity.Information, f.Code.ToString(), f.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["succeeded"] = result.Succeeded.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["previousEssence"] = result.PreviousEssence.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["newEssence"] = result.NewEssence.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["newState"] = result.NewState is null ? string.Empty : System.Text.Json.JsonSerializer.Serialize(result.NewState, JsonOptions),
                    ["newStateVersion"] = result.NewStateVersion?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
                });
        }

        private static RuleSetOperationResult ExecuteCharm(RuleSetOperationRequest request)
        {
            if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
                !request.Inputs.TryGetValue("currentState", out var stateJson) ||
                !request.Inputs.TryGetValue("expectedStateVersion", out var versionText) ||
                !request.Inputs.TryGetValue("charmKey", out var charmKey) ||
                !int.TryParse(versionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
            {
                return InvalidSpiritRequest("Charm execution requires requestId, currentState, expectedStateVersion, charmKey.");
            }

            var state = System.Text.Json.JsonSerializer.Deserialize<WerewolfSpiritRuntimeState>(stateJson, JsonOptions);
            if (state is null)
            {
                return InvalidSpiritRequest("Failed to deserialize spirit state.");
            }

            var gnosisCost = int.TryParse(request.Inputs.GetValueOrDefault("gnosisCost"), System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var gc) ? gc : (int?)null;
            var essenceCost = int.TryParse(request.Inputs.GetValueOrDefault("essenceCost"), System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var ec) ? ec : (int?)null;
            var charmRequest = new CharmExecutionRequest(state, expectedVersion, requestId, charmKey, gnosisCost, essenceCost);
            var result = WerewolfSpiritMechanicServices.ExecuteCharm(charmRequest);

            return new RuleSetOperationResult(
                result.Succeeded,
                result.Succeeded ? null : RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(f => new RuleSetRuntimeFinding(f.Severity == SpiritMechanicFindingSeverity.Error ? RuleSetRuntimeFindingSeverity.Error : RuleSetRuntimeFindingSeverity.Information, f.Code.ToString(), f.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["succeeded"] = result.Succeeded.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["executedCharmKey"] = result.ExecutedCharmKey ?? string.Empty,
                    ["effectDescription"] = result.EffectDescription ?? string.Empty,
                    ["newState"] = result.NewState is null ? string.Empty : System.Text.Json.JsonSerializer.Serialize(result.NewState, JsonOptions),
                    ["newStateVersion"] = result.NewStateVersion?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
                });
        }

        private static RuleSetOperationResult ExecuteEvaluateCommand(RuleSetOperationRequest request)
        {
            if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
                !request.Inputs.TryGetValue("currentState", out var stateJson) ||
                !request.Inputs.TryGetValue("expectedStateVersion", out var versionText) ||
                !request.Inputs.TryGetValue("charisma", out var charismaText) ||
                !request.Inputs.TryGetValue("leadership", out var leadershipText) ||
                !request.Inputs.TryGetValue("targetWillpower", out var targetWillpowerText) ||
                !request.Inputs.TryGetValue("diceValues", out var diceText) ||
                !int.TryParse(versionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
                !int.TryParse(charismaText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var charisma) ||
                !int.TryParse(leadershipText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var leadership) ||
                !int.TryParse(targetWillpowerText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var targetWillpower))
            {
                return InvalidSpiritRequest("Command requires requestId, currentState, expectedStateVersion, charisma, leadership, targetWillpower, diceValues.");
            }

            var state = System.Text.Json.JsonSerializer.Deserialize<WerewolfSpiritRuntimeState>(stateJson, JsonOptions);
            if (state is null)
            {
                return InvalidSpiritRequest("Failed to deserialize spirit state.");
            }

            var diceValues = ParseCsv(diceText).Select(v => int.TryParse(v, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0).ToArray();
            var commandRequest = new CommandRequest(state, expectedVersion, requestId, charisma, leadership, targetWillpower, diceValues);
            var result = WerewolfSpiritMechanicServices.EvaluateCommand(commandRequest);

            return new RuleSetOperationResult(
                result.Succeeded,
                result.Succeeded ? null : RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(f => new RuleSetRuntimeFinding(f.Severity == SpiritMechanicFindingSeverity.Error ? RuleSetRuntimeFindingSeverity.Error : RuleSetRuntimeFindingSeverity.Information, f.Code.ToString(), f.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["succeeded"] = result.Succeeded.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["successes"] = result.Successes.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["isCommanded"] = result.IsCommanded.ToString(System.Globalization.CultureInfo.InvariantCulture)
                });
        }

        private static RuleSetOperationResult ExecuteEvaluatePossession(RuleSetOperationRequest request)
        {
            if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
                !request.Inputs.TryGetValue("currentState", out var stateJson) ||
                !request.Inputs.TryGetValue("expectedStateVersion", out var versionText) ||
                !request.Inputs.TryGetValue("targetWillpower", out var targetWillpowerText) ||
                !request.Inputs.TryGetValue("diceValues", out var diceText) ||
                !int.TryParse(versionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
                !int.TryParse(targetWillpowerText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var targetWillpower))
            {
                return InvalidSpiritRequest("Possession requires requestId, currentState, expectedStateVersion, targetWillpower, diceValues.");
            }

            var state = System.Text.Json.JsonSerializer.Deserialize<WerewolfSpiritRuntimeState>(stateJson, JsonOptions);
            if (state is null)
            {
                return InvalidSpiritRequest("Failed to deserialize spirit state.");
            }

            var diceValues = ParseCsv(diceText).Select(v => int.TryParse(v, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0).ToArray();
            var possessionRequest = new PossessionRequest(state, expectedVersion, requestId, targetWillpower, diceValues);
            var result = WerewolfSpiritMechanicServices.EvaluatePossession(possessionRequest);

            return new RuleSetOperationResult(
                result.Succeeded,
                result.Succeeded ? null : RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(f => new RuleSetRuntimeFinding(f.Severity == SpiritMechanicFindingSeverity.Error ? RuleSetRuntimeFindingSeverity.Error : RuleSetRuntimeFindingSeverity.Information, f.Code.ToString(), f.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["succeeded"] = result.Succeeded.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["successes"] = result.Successes.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["isPossessing"] = result.IsPossessing.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["duration"] = result.Duration.ToString()
                });
        }

        private static RuleSetOperationResult ExecuteApplySpiritDamage(RuleSetOperationRequest request)
        {
            if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
                !request.Inputs.TryGetValue("currentState", out var stateJson) ||
                !request.Inputs.TryGetValue("expectedStateVersion", out var versionText) ||
                !request.Inputs.TryGetValue("damageAmount", out var damageText) ||
                !request.Inputs.TryGetValue("difficulty", out var difficultyText) ||
                !int.TryParse(versionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
                !int.TryParse(damageText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var damageAmount) ||
                !int.TryParse(difficultyText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var difficulty))
            {
                return InvalidSpiritRequest("Spirit damage requires requestId, currentState, expectedStateVersion, damageAmount, difficulty.");
            }

            var state = System.Text.Json.JsonSerializer.Deserialize<WerewolfSpiritRuntimeState>(stateJson, JsonOptions);
            if (state is null)
            {
                return InvalidSpiritRequest("Failed to deserialize spirit state.");
            }

            var isAggravated = bool.TryParse(request.Inputs.GetValueOrDefault("isAggravated"), out var a) && a;
            var damageRequest = new SpiritDamageRequest(state, expectedVersion, requestId, damageAmount, difficulty, isAggravated);
            var result = WerewolfSpiritMechanicServices.ApplyDamage(damageRequest);

            return new RuleSetOperationResult(
                result.Succeeded,
                result.Succeeded ? null : RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(f => new RuleSetRuntimeFinding(f.Severity == SpiritMechanicFindingSeverity.Error ? RuleSetRuntimeFindingSeverity.Error : RuleSetRuntimeFindingSeverity.Information, f.Code.ToString(), f.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["succeeded"] = result.Succeeded.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["damageApplied"] = result.DamageApplied.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["essenceLost"] = result.EssenceLost.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["isAtDeathBoundary"] = result.IsAtDeathBoundary.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["newState"] = result.NewState is null ? string.Empty : System.Text.Json.JsonSerializer.Serialize(result.NewState, JsonOptions),
                    ["newStateVersion"] = result.NewStateVersion?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
                });
        }

        private static RuleSetOperationResult InvalidSpiritRequest(string message)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidSpiritRequest", message)],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        private static RuleSetOperationResult ExecuteSpiritLocation(RuleSetOperationRequest request)
        {
            if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
                !request.Inputs.TryGetValue("spiritId", out var spiritId) ||
                !request.Inputs.TryGetValue("realmKey", out var realmKey))
            {
                return InvalidSpiritRequest("Spirit location requires requestId, spiritId, realmKey.");
            }

            var layerKey = request.Inputs.GetValueOrDefault("layerKey") ?? string.Empty;
            var locationStateTransition = request.Inputs.GetValueOrDefault("locationStateTransition") ?? string.Empty;

            var boundary = new WerewolfSpiritLocationBoundaryPayload(
                SpiritId: spiritId,
                RealmKey: realmKey,
                LayerKey: layerKey,
                GauntletReference: string.Empty,
                LocationStateTransition: locationStateTransition,
                ChronicleOrchestrationRequired: "Chronicle must orchestrate location state transition",
                SourceLocator: "Lines 3384, 3462",
                Note: "S5 boundary: spirit location in scene/realm is owned by Chronicle.");

            return new RuleSetOperationResult(
                true,
                null,
                [],
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["succeeded"] = true.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["boundary"] = System.Text.Json.JsonSerializer.Serialize(boundary, JsonOptions)
                });
        }

        private static RuleSetOperationResult ExecuteGauntletLookup(RuleSetOperationRequest request)
        {
            if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
                !request.Inputs.TryGetValue("locationCategoryKey", out var locationCategoryKey) ||
                !request.Inputs.TryGetValue("gauntletValue", out var gauntletText) ||
                !int.TryParse(gauntletText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var gauntletValue) ||
                !request.Inputs.TryGetValue("películaValue", out var películaText) ||
                !int.TryParse(películaText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var películaValue))
            {
                return InvalidSpiritRequest("Gauntlet lookup requires requestId, locationCategoryKey, gauntletValue, películaValue.");
            }

            if (gauntletValue < 2 || gauntletValue > 9)
            {
                return InvalidSpiritRequest($"Gauntlet value {gauntletValue} is outside typical range 2-9.");
            }

            var boundary = new WerewolfGauntletLookupBoundaryPayload(
                LocationCategoryKey: locationCategoryKey,
                LocationReference: string.Empty,
                GauntletValue: gauntletValue,
                PelículaValue: películaValue,
                SourceLocator: "Lines 3235-3249",
                Note: "S5 boundary: location-bound Gauntlet lookup requires Chronicle location context.");

            return new RuleSetOperationResult(
                true,
                null,
                [],
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["succeeded"] = true.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["boundary"] = System.Text.Json.JsonSerializer.Serialize(boundary, JsonOptions)
                });
        }

        private static RuleSetOperationResult ExecuteRealmTravel(RuleSetOperationRequest request)
        {
            if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
                !request.Inputs.TryGetValue("spiritId", out var spiritId) ||
                !request.Inputs.TryGetValue("originRealmKey", out var originRealmKey) ||
                !request.Inputs.TryGetValue("destinationRealmKey", out var destinationRealmKey))
            {
                return InvalidSpiritRequest("Realm travel requires requestId, spiritId, originRealmKey, destinationRealmKey.");
            }

            var travelPath = request.Inputs.GetValueOrDefault("travelPath") ?? string.Empty;

            var boundary = new WerewolfRealmTravelBoundaryPayload(
                SpiritId: spiritId,
                OriginRealmKey: originRealmKey,
                DestinationRealmKey: destinationRealmKey,
                TravelPath: travelPath,
                EligibilityResult: "Unknown from source",
                ChronicleOrchestrationRequired: "Chronicle must orchestrate realm/path persistence and scene transition",
                SourceLocator: "Lines 3376-3382",
                Note: "S5 boundary: realm travel via Moon Trails, Spirit Trails, Portals, Webs, Wyrm Tunnels. No deterministic travel mechanics defined in source.");

            return new RuleSetOperationResult(
                true,
                null,
                [],
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["succeeded"] = true.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["boundary"] = System.Text.Json.JsonSerializer.Serialize(boundary, JsonOptions)
                });
        }

        private static RuleSetOperationResult ExecuteScenePresence(RuleSetOperationRequest request)
        {
            if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
                !request.Inputs.TryGetValue("spiritId", out var spiritId) ||
                !request.Inputs.TryGetValue("presenceState", out var presenceState))
            {
                return InvalidSpiritRequest("Scene presence requires requestId, spiritId, presenceState.");
            }

            var sceneReference = request.Inputs.GetValueOrDefault("sceneReference") ?? string.Empty;

            var boundary = new WerewolfScenePresenceBoundaryPayload(
                SpiritId: spiritId,
                SceneReference: sceneReference,
                PresenceState: presenceState,
                ObservableState: string.Empty,
                ChronicleOrchestrationRequired: "Chronicle must orchestrate scene entity placement",
                SourceLocator: "Lines 3200, 3384",
                Note: "S5 boundary: spirit presence/absence in Chronicle scene is owned by Chronicle.");

            return new RuleSetOperationResult(
                true,
                null,
                [],
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["succeeded"] = true.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["boundary"] = System.Text.Json.JsonSerializer.Serialize(boundary, JsonOptions)
                });
        }

        private static RuleSetOperationResult ExecuteCaernPelícula(RuleSetOperationRequest request)
        {
            if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
                !request.Inputs.TryGetValue("caernLevel", out var caernLevelText) ||
                !int.TryParse(caernLevelText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var caernLevel))
            {
                return InvalidSpiritRequest("Caern Película requires requestId, caernLevel.");
            }

            if (caernLevel < 1 || caernLevel > 5)
            {
                return InvalidSpiritRequest("Caern level must be between 1 and 5.");
            }

            var (películaLevel, moonBridgeKm) = caernLevel switch
            {
                1 => (3, 50),
                2 => (2, 100),
                3 => (1, 200),
                4 => (1, 500),
                5 => (0, 1000),
                _ => (0, 0)
            };

            var boundary = new WerewolfCaernPelículaBoundaryPayload(
                CaernReference: string.Empty,
                CaernLevel: caernLevel,
                PelículaLevel: películaLevel,
                MoonBridgeMaxDistanceKm: moonBridgeKm,
                SourceLocator: "Lines 3249-3255",
                Note: "S5 boundary: deterministic Caern Película table materialized. Chronicle must bind table to world Caern entities.");

            return new RuleSetOperationResult(
                true,
                null,
                [],
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["succeeded"] = true.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["boundary"] = System.Text.Json.JsonSerializer.Serialize(boundary, JsonOptions)
                });
        }

        private static RuleSetOperationResult ExecutePackTotemLink(RuleSetOperationRequest request)
        {
            if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
                !request.Inputs.TryGetValue("packId", out var packId) ||
                !request.Inputs.TryGetValue("totemId", out var totemId))
            {
                return InvalidSpiritRequest("Pack Totem link requires requestId, packId, totemId.");
            }

            var linkState = request.Inputs.GetValueOrDefault("linkState") ?? string.Empty;

            var boundary = new WerewolfPackTotemLinkBoundaryPayload(
                PackId: packId,
                TotemId: totemId,
                LinkState: linkState,
                BenefitScope: string.Empty,
                ChronicleOrchestrationRequired: "Chronicle must orchestrate Pack/Totem persistent linkage",
                SourceLocator: "Lines 1632, 1636",
                Note: "S5 boundary: Pack-Totem connection enables shared Totem benefits. Persistent linkage is owned by Chronicle.");

            return new RuleSetOperationResult(
                true,
                null,
                [],
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["succeeded"] = true.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["boundary"] = System.Text.Json.JsonSerializer.Serialize(boundary, JsonOptions)
                });
        }

        private static RuleSetOperationResult ExecuteSharedTotemEffects(RuleSetOperationRequest request)
        {
            if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
                !request.Inputs.TryGetValue("totemId", out var totemId) ||
                !request.Inputs.TryGetValue("effectKeys", out var effectKeysJson))
            {
                return InvalidSpiritRequest("Shared Totem effects requires requestId, totemId, effectKeys.");
            }

            var intendedRecipients = request.Inputs.GetValueOrDefault("intendedRecipients") ?? string.Empty;

            try
            {
                var effectKeys = System.Text.Json.JsonSerializer.Deserialize<List<string>>(effectKeysJson, JsonOptions);
                if (effectKeys is null || effectKeys.Count == 0)
                {
                    return InvalidSpiritRequest("Effect keys must not be empty.");
                }

                var boundary = new WerewolfSharedTotemEffectsBoundaryPayload(
                    TotemId: totemId,
                    EffectKeys: effectKeys,
                    IntendedRecipients: intendedRecipients,
                    ApplicationScope: "Pack members",
                    ChronicleOrchestrationRequired: "Chronicle must orchestrate per-turn benefit distribution across Pack aggregate",
                    SourceLocator: "Lines 1636, 1646",
                    Note: "S5 boundary: Totem benefits available to Pack members per turn. Distribution is owned by Chronicle.");

                return new RuleSetOperationResult(
                    true,
                    null,
                    [],
                    new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["succeeded"] = true.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        ["boundary"] = System.Text.Json.JsonSerializer.Serialize(boundary, JsonOptions)
                    });
            }
            catch (System.Text.Json.JsonException)
            {
                return InvalidSpiritRequest("Failed to deserialize effectKeys.");
            }
        }
    }
