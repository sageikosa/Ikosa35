using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Displacement : SpellDef, ISpellMode, IDurableCapable, ISaveCapable, IProcessFeedback
    {
        public override string DisplayName => @"Displacement";
        public override string Description => @"Target gains 50% miss chance against attack rolls";
        public override MagicStyle MagicStyle => new Illusion(Illusion.SubIllusion.Glamer);

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new MaterialComponent();
                yield break;
            }
        }
        #endregion

        #region public override IEnumerable<SpellComponent> DivineComponents { get; }
        public override IEnumerable<SpellComponent> DivineComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new DivineFocusComponent();
                yield break;
            }
        }
        #endregion

        #region public override IEnumerable<ISpellMode> SpellModes { get; }
        public override IEnumerable<ISpellMode> SpellModes
        {
            get
            {
                yield return this;
                yield break;
            }
        }
        #endregion

        #region ISpellMode
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Target", @"Target", Lethality.AlwaysNonLethal, 20, this,
                FixedRange.One, FixedRange.One, new MeleeRange(), new CreatureTargetType());
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Target", 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // add the fully targetted effect
            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        #region IDurableMode
        public IEnumerable<int> DurableSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            (target as Creature)?.AddIInteractHandler(this);
            return this;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (target as Creature)?.RemoveIInteractHandler(source.ActiveAdjunctObject as IInteractHandler);
        }

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
            => @"Save.Will";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Round(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            // NONE
            yield break;
        }

        public bool IsDismissable(int subMode)
            => true;
        #endregion

        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource,Interaction workSet, string saveKey)
            => new SaveMode(SaveType.Will, SaveEffect.Negates,
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));

        #region IInteractHandler Members
        public void HandleInteraction(Interaction workSet)
        {
            return;
        }

        #region public IEnumerable<Type> GetInteractionTypes()
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(MeleeAttackData);
            yield return typeof(ReachAttackData);
            yield return typeof(RangedAttackData);
            yield break;
        }
        #endregion

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // put just before the attack handler (so it is processes feedback before the transit attack handler)
            if (typeof(CreatureAttackHandler) == existingHandler.GetType())
            {
                return true;
            }

            return false;
        }
        #endregion

        #region IProcessFeedback Members
        public void ProcessFeedback(Interaction workSet)
        {
            // if we didn't hit, there's no need to check miss chance
            var _feedback = workSet.Feedback.OfType<AttackFeedback>().FirstOrDefault();
            if (!(_feedback?.Hit ?? false))
            {
                return;
            }

            if ((workSet.InteractData is AttackData _atk)
                && (_atk.Attacker is Creature _attacker))
            {
                var _loc = _attacker.GetLocated()?.Locator;
                var _tLoc = (workSet.Target as IAdjunctable)?.GetLocated()?.Locator;
                if ((_loc != null) && (_tLoc != null))
                {
                    // does a true sight sense work here?
                    if (_attacker.Senses.BestSenses.OfType<TrueSight>()
                        .Any(_s => _s.CarrySenseInteraction(_loc.Map, _loc.GeometricRegion,
                        _tLoc.GeometricRegion, ITacticalInquiryHelper.EmptyArray)))
                    {
                        // able to carry an interaction
                        return;
                    }
                }

                var _miss = MissChanceAlteration.GetMissChance(workSet, this);
                if (_miss.PercentRolled <= 50)
                {
                    // rolled a miss, see if a second change is possible
                    if (!_miss.SecondRoll
                       && (_attacker?.Feats.Contains(typeof(BlindFight)) ?? false))
                    {
                        // yes, so do-over
                        _miss.SecondChance(DieRoller.RollDie(_tLoc?.ICore.ID ?? Guid.Empty, 100, DisplayName, @"Miss Chance"));
                        if (_miss.PercentRolled <= 50)
                        {
                            // definitely missed
                            _feedback.Hit = false;
                            _feedback.CriticalHit = false;
                        }
                    }
                    else
                    {
                        // no second chance: missed
                        _feedback.Hit = false;
                        _feedback.CriticalHit = false;
                    }
                }
            }
        }
        #endregion
    }
}
