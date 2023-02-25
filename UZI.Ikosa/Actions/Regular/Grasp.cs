using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class Grasp : ActionBase, IAttackSource
    {
        public Grasp(IActionSource source, ActionTime time, string orderKey)
            : base(source, time, false, true, orderKey)
        {
            _Zero = new DeltableQualifiedDelta(0, @"Touch", this);
        }

        private DeltableQualifiedDelta _Zero;

        public override string Key => @"Grasp";
        public override string DisplayName(CoreActor actor) => @"Grasp to Sense";

        /// <summary>Grasp action can have typically one (or more for larger creatures) follow-ups per round</summary>
        public override bool IsStackBase(CoreActivity activity)
            => TimeCost.ActionTimeType == TimeType.Regular;

        #region public override bool PopStack(CoreActionBudget budget, CoreActivity activity)
        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity)
        {
            // do not pop if this is stacked on another grasp action
            if (budget.TopActivity?.Action is Grasp)
                return false;
            return base.WillClearStack(budget, activity);
        }
        #endregion

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Grasp", activity.Actor, observer);

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
            // NOTE: no criticals on grasping (no damage)
            yield return new CellAttackAim(@"Cell", @"Cell", Lethality.AlwaysNonLethal, 100, null,
                FixedRange.One, FixedRange.One, new MeleeRange())
            { UseHiddenRolls = true };
            yield break;
        }
        #endregion

        #region IAttackSource Members

        public DeltableQualifiedDelta AttackBonus => _Zero;
        public int CriticalRange => 20;
        public DeltableQualifiedDelta CriticalRangeFactor => _Zero;
        public DeltableQualifiedDelta CriticalDamageFactor => _Zero;
        public IEnumerable<StepPrerequisite> AttackResultPrerequisites(Interaction workSet) { yield break; }

        public void AttackResult(AttackResultStep result, Interaction workSet)
        {
            var _graspBudget = GraspProbeBudget.GetBudget(_Budget);
            if (_graspBudget != null)
                _graspBudget.UseGrasp();

            var _activity = result.TargetingProcess as CoreActivity;
            void _notify(string message)
                => result.AppendFollowing((result.TargetingProcess as CoreActivity).GetActivityResultNotifyStep(message));
            var _atkBack = workSet.Feedback.OfType<AttackFeedback>().FirstOrDefault();
            var _critter = _Budget.Actor as Creature;
            var _locator = _critter.GetLocated().Locator;
            if ((workSet.InteractData is MeleeAttackData _atk) && (_atkBack != null))
            {
                if (_atkBack.Hit)
                {
                    var _graspData = new GraspData(_critter, _critter, _locator, new CellLocation(_atk.TargetCell));
                    var _graspSet = new Interaction(_critter, ActionSource, workSet.Target, _graspData);
                    if (workSet.Target != null)
                    {
                        // target handles grasp
                        workSet.Target.HandleInteraction(_graspSet);
                        var _infoBack = _graspSet.Feedback.OfType<GraspFeedback>().FirstOrDefault();

                        // actor gets to know it grasped
                        //result.AppendFollowing(result.Activity.
                        result.AppendFollowing(
                            _activity.GetActivityResultNotifyStep(
                                _infoBack != null ? _infoBack.Information.ToArray()
                                : new Info[] { new Info { Message = @"Contact" } }));

                        // target gets to know it was grasped
                        if (workSet.Target is CoreActor _actor)
                        {
                            result.EnqueueNotify(new AttackedNotify(_actor.ID, @"Grasped",
                                _activity.GetActivityInfo(_actor)), _actor.ID);
                        }

                        if ((_infoBack != null)
                            && (_infoBack.ActionAwarnesses != null)
                            && _infoBack.ActionAwarnesses.Any())
                        {
                            GraspAwareness.CreateGraspAwareness(
                                _critter,
                                workSet.Target as IAdjunctable,
                                _infoBack.ActionAwarnesses);
                        }
                    }
                    else
                    {
                        _notify(@"Contact");
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
                        var _map = _locator.Map;
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
                                        // no contact
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

        public bool IsSourceChannel(IAttackSource source)
            => (source == this);

        #endregion

        // IAdjunctSet Members
        public AdjunctSet Adjuncts
            => _Budget?.Actor?.Adjuncts;

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
        {
            if (ProvokesTarget)
            {
                var _loc = potentialTarget?.GetLocated()?.Locator;
                if (_loc != null)
                {
                    foreach (var _target in activity.Targets.OfType<AttackTarget>()
                        .Where(_t => _t.Key.Equals(@"Cell", StringComparison.OrdinalIgnoreCase)))
                    {
                        if (_target.Target.ID == potentialTarget.ID)
                            return true;

                        if ((_target.Target as IAdjunctable)?.GetLocated()?.Locator == _loc)
                            return true;
                    }
                }
            }
            return false;
        }

    }
}