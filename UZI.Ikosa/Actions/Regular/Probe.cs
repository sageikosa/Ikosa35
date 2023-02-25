using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class Probe : ActionBase, IAttackSource, IRangedSourceProvider
    {
        public Probe(IWeaponHead source, ActionTime time, string orderKey)
            : base(source, time, false, false, orderKey)
        {
        }

        public override string Key => @"Probe";
        public override string DisplayName(CoreActor actor) => @"Probe to Sense";

        public IWeaponHead WeaponHead => Source as IWeaponHead;
        public IWeapon Weapon => WeaponHead.ContainingWeapon;
        public IMeleeWeapon MeleeWeapon => Weapon as IMeleeWeapon;

        /// <summary>Grasp action can have typically one (or more for larger creatures) follow-ups per round</summary>
        public override bool IsStackBase(CoreActivity activity)
            => TimeCost.ActionTimeType == TimeType.Regular;

        #region public override bool PopStack(CoreActionBudget budget, CoreActivity activity)
        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity)
        {
            // do not pop if this is stacked on another probe action
            if (budget.TopActivity?.Action is Probe)
                return false;
            return base.WillClearStack(budget, activity);
        }
        #endregion

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Probe", activity.Actor, observer);

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _graspBudget = GraspProbeBudget.GetBudget(_Budget);
            if (_graspBudget == null)
            {
                _graspBudget = new GraspProbeBudget(2);
                _Budget.BudgetItems.Add(_graspBudget.Source, _graspBudget);
            }

            activity.EnqueueRegisterPreEmptively(Budget);
            return new AttackStep(activity, this);
        }
        #endregion

        #region public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            // NOTE: miss chance will be automatic
            // NOTE: no criticals on probing (no damage)
            yield return new CellAttackAim(@"Cell", @"Cell", Lethality.AlwaysNonLethal, 100, this, FixedRange.One, FixedRange.One,
                new StrikeZoneRange(MeleeWeapon))
            { UseHiddenRolls = true };
            yield break;
        }
        #endregion

        #region IAttackSource Members

        public DeltableQualifiedDelta AttackBonus => WeaponHead.AttackBonus;
        public int CriticalRange => WeaponHead.CriticalRange;
        public DeltableQualifiedDelta CriticalRangeFactor => WeaponHead.CriticalRangeFactor;
        public DeltableQualifiedDelta CriticalDamageFactor => WeaponHead.CriticalDamageFactor;
        public IEnumerable<StepPrerequisite> AttackResultPrerequisites(Interaction workSet) { yield break; }

        #region public void AttackResult(AttackResultStep result, Interaction workSet)
        public void AttackResult(AttackResultStep result, Interaction workSet)
        {
            var _graspBudget = GraspProbeBudget.GetBudget(_Budget);
            if (_graspBudget != null)
                _graspBudget.UseGrasp();

            var _activity = result.TargetingProcess as CoreActivity;
            void _notify(string message)
                => result.AppendFollowing((result.TargetingProcess as CoreActivity).GetActivityResultNotifyStep(message));
            var _atkBack = workSet.Feedback.OfType<AttackFeedback>().FirstOrDefault();
            if ((workSet.InteractData is MeleeAttackData _atk) && (_atkBack != null))
            {
                if (_atkBack.Hit)
                {
                    // successful probe
                    result.AppendFollowing(_activity.GetActivityResultNotifyStep(@"Contact"));
                    if (workSet.Target is CoreActor _actor)
                    {
                        result.EnqueueNotify(new AttackedNotify(_actor.ID, @"Probed",
                            _activity.GetActivityInfo(_actor)),
                            _actor.ID);
                    }
                }
                else
                {
                    var _miss = workSet.InteractData.Alterations.OfType<MissChanceAlteration>().FirstOrDefault();
                    if (_miss != null)
                    {
                        // miss chance
                        _notify(@"No Contact");
                    }
                    else if (_atkBack.NoLines)
                    {
                        // no lines
                        _notify(@"Early Contact");
                    }
                    else
                    {
                        var _critter = _Budget.Actor as Creature;
                        var _map = _critter.GetLocated().Locator.Map;
                        var _visualizer = _critter.GetTerrainVisualizer();
                        var _effect = _map.GetVisualEffect(new CellLocation(_atk.SourceCell), _atk.TargetCell, _visualizer);
                        switch (_effect)
                        {
                            case Visualize.VisualEffect.Unseen:
                            case Visualize.VisualEffect.Skip:
                            case Visualize.VisualEffect.Highlighted:
                                {
                                    // no visibility at target cell
                                    if (_atk.Alterations.OfType<CoverAlteration>().Any())
                                    {
                                        // cover?
                                        _notify(@"Contact");
                                    }
                                    else
                                    {
                                        _notify(@"No Contact");
                                    }
                                }
                                break;

                            default:
                                // visibility
                                _notify(@"No Contact");
                                break;
                        }
                    }
                }
            }
            else
            {
                _notify(@"No Contact");
            }
        }
        #endregion

        public bool IsSourceChannel(IAttackSource source)
            => (source == this)
            || (source == WeaponHead);

        #endregion

        #region IAdjunctSet Members

        public AdjunctSet Adjuncts
            => WeaponHead.Adjuncts;

        #endregion

        public IRangedSource GetRangedSource(CoreActor actor, ActionBase action, RangedAim aim, IInteract target)
            => Weapon as IRangedSource;

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
