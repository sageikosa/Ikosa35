using System;
using System.Collections.Generic;
using System.Reflection;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    public class IkosaKnownTypes
    {
        private static IEnumerable<Type> KnownTypes(ICustomAttributeProvider provider)
        {
            foreach (var _type in IkosaServiceInfos.InfoTypes())
                yield return _type;

            // deltables
            yield return typeof(AbilityInfo);
            yield return typeof(SkillInfo);

            // prereqs
            yield return typeof(ActionInquiryPrerequisiteInfo);
            yield return typeof(AimTargetPrerequisiteInfo);
            yield return typeof(CheckPrerequisiteInfo);
            yield return typeof(ChoicePrerequisiteInfo);
            yield return typeof(CoreSelectPrerequisiteInfo);
            yield return typeof(OpportunisticPrerequisiteInfo);
            yield return typeof(PromptTurnTrackerPrerequisiteInfo);
            yield return typeof(ReactivePrerequisiteInfo);
            yield return typeof(RollPrerequisiteInfo);
            yield return typeof(SavePrerequisiteInfo);
            yield return typeof(WaitReleasePrerequisiteInfo);

            // aiming modes
            yield return typeof(AttackAimInfo);
            yield return typeof(AutoSpellResistanceAimInfo);
            yield return typeof(AwarenessAimInfo);
            yield return typeof(CharacterStringAimInfo);
            yield return typeof(CoinTradeInfo);
            yield return typeof(CoinTypeInfo);
            yield return typeof(CoreListAimInfo);
            yield return typeof(EnvironmentAimInfo);
            yield return typeof(FixedAimInfo);
            yield return typeof(LocationAimInfo);
            yield return typeof(MovementAimInfo);
            yield return typeof(ObjectListAimInfo);
            yield return typeof(OptionAimInfo);
            yield return typeof(PersonalAimInfo);
            yield return typeof(PersonalConicAimInfo);
            yield return typeof(PersonalStartAimInfo);
            yield return typeof(PrepareSpellSlotsAimInfo);
            yield return typeof(QuantitySelectAimInfo);
            yield return typeof(RangedAimInfo);
            yield return typeof(RollAimInfo);
            yield return typeof(RotateAimInfo);
            yield return typeof(SlideAimInfo);
            yield return typeof(SuccessCheckAimInfo);
            yield return typeof(TiltAimInfo);
            yield return typeof(TradeExchangeAimInfo);
            yield return typeof(VolumeAimInfo);
            yield return typeof(WallSurfaceAimInfo);
            yield return typeof(WordAimInfo);

            // targets
            yield return typeof(AttackTargetInfo);
            yield return typeof(AwarenessTargetInfo);
            yield return typeof(CharacterStringTargetInfo);
            yield return typeof(CoreInfoTargetInfo);
            yield return typeof(CubicTargetInfo);
            yield return typeof(FixedAimTargetInfo);
            yield return typeof(GeometricTargetInfo);
            yield return typeof(HeadingTargetInfo);
            yield return typeof(LocationTargetInfo);
            yield return typeof(MultiStepDestinationInfo);
            yield return typeof(OptionTargetInfo);
            yield return typeof(PrepareSpellSlotsTargetInfo);
            yield return typeof(QuantitySelectTargetInfo);
            yield return typeof(SuccessCheckTargetInfo);
            yield return typeof(ValueIntTargetInfo);
            yield return typeof(WallSurfaceTargetInfo);

            // other stuff (mainly because CoreInfo aiming can return many things)
            yield return typeof(AmmoBundleInfo);
            yield return typeof(AmmoInfo);
            yield return typeof(WeaponHeadInfo);

            // extra info
            yield return typeof(ExtraInfoMarkerInfo);

            // various spell prep
            yield return typeof(ClassSpellInfo);
            yield return typeof(PreparedSpellSlotInfo);
            yield return typeof(SpellSlotSetInfo);
            yield return typeof(SpellSlotLevelInfo);
            yield return typeof(SpellSlotInfo);

            // documents
            yield return typeof(DocImageContent);
            yield return typeof(DocTextContent);
            yield return typeof(BookSpellInfo);

            // target types
            yield return typeof(CreatureTargetTypeInfo);
            yield return typeof(LivingCreatureTargetTypeInfo);
            yield return typeof(ObjectTargetTypeInfo);

            // budget items
            yield return typeof(FlightBudgetInfo);
            yield return typeof(MovementBudgetInfo);
            yield return typeof(MovementRangeBudgetInfo);
            yield return typeof(CapacityBudgetInfo);
            yield return typeof(AdjunctBudgetInfo);

            // ranges
            yield return typeof(DiceRangeInfo);
            yield return typeof(MeleeRangeInfo);
            yield return typeof(StrikeZoneRangeInfo);

            // damage
            yield return typeof(AbilityDamageInfo);
            yield return typeof(ConditionDamageInfo);
            yield return typeof(DamageInfo);
            yield return typeof(DamageType);
            yield return typeof(DealingDamageNotify);
            yield return typeof(EnergyType);
            yield return typeof(EnergyDamageInfo);
            yield return typeof(LethalDamageInfo);
            yield return typeof(NonLethalDamageInfo);

            // Action Info
            yield return typeof(PowerActionInfo);
            yield return typeof(ObservedActivityInfo); 

            // SysNotify
            yield return typeof(ActivityResultNotify);
            yield return typeof(AttackedNotify);
            yield return typeof(BadNewsNotify);
            yield return typeof(CheckNotify);
            yield return typeof(CheckResultNotify);
            yield return typeof(ConditionNotify);
            yield return typeof(DealingDamageNotify);
            yield return typeof(DealtDamageNotify);
            yield return typeof(DeltaCalcNotify);
            yield return typeof(GoodNewsNotify);
            yield return typeof(RefreshNotify);
            yield return typeof(RollNotify);
            yield return typeof(SysNotify);
            yield break;
        }
    }
}
