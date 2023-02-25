using System;
using System.Linq;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Actions;
using System.Diagnostics;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class Overrun : MoveSubAct
    {
        public Overrun(MovementBase movement, ActionTime actionTime, bool continuing)
            : base(movement, actionTime, true)
        {
            // may need regular, depends on whether opponent blocks...
            _NeedsTime = new ActionTime(TimeType.Regular);
            _Continuing = continuing;
        }

        #region data
        private bool _Continuing;
        #endregion

        public override string Key => @"Movement.Overrun";
        public override string DisplayName(CoreActor actor) => $@"Overrun ({Movement.Name})";
        public bool IsContinuing => _Continuing;

        public override CoreActivity NextMoveInSequence(CoreActivity activity, List<AimTarget> targets)
            => new CoreActivity(activity.Actor, new Overrun(Movement, new ActionTime(TimeType.SubAction), true), targets);

        protected override IEnumerable<CoreStep> PreMoveSteps(MoveCostCheckStep step)
        {
            yield break;
        }

        protected override IEnumerable<CoreStep> NormalMoveSteps(MoveCostCheckStep step, double finalCost, bool atStart)
        {
            return DiagonalOnMoveCostCheck(step, finalCost, atStart);
        }

        #region protected override IEnumerable<CoreStep> PostMoveSteps(MoveCostCheckStep step)
        protected override IEnumerable<CoreStep> PostMoveSteps(MoveCostCheckStep step)
        {
            // if there's an overrunning target, get it
            var _overrunning = Budget.BudgetItems[typeof(OverrunningBudget)] as OverrunningBudget;
            if (Budget.BudgetItems[typeof(OverrunBudget)] == null)
            {
                // register overrun attempt in budget, only one attempt can be made per round
                yield return new RegisterOverrun(step.Activity);
            }

            // NOTE: tacked onto the end, so that the movement happens first, 
            yield return new TryOverrun(step.Activity, _overrunning);

            // done
            yield break;
        }
        #endregion

        #region public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            foreach (var _aim in base.AimingMode(activity))
            {
                // NOTE: no double move option, need the action budget for the potential regular action
                if (!(_aim is OptionAim _opt) || !_opt.Key.Equals(@"Double", StringComparison.OrdinalIgnoreCase))
                    yield return _aim;
            }
            yield break;
        }
        #endregion

        #region public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        {
            // must have a local budget to overrun
            if (!(budget is LocalActionBudget _local))
                return new ActivityResponse(false);
            if (!IsContinuing && (_local.BudgetItems[typeof(OverrunBudget)] != null))
            {
                // since the budget is attached, it has been used once already
                return new ActivityResponse(false);
            }

            // base functionality
            return base.CanPerformNow(budget);
        }
        #endregion

        #region public static void DoOverrun(CoreStep overrunStep, RollPrerequisite atkRoll, RollPrerequisite defRoll)
        public static void DoOverrun(CoreStep overrunStep, RollPrerequisite atkRoll, RollPrerequisite defRoll, bool canCounter)
        {
            // overrun qualifier
            var _pusher = atkRoll.Fulfiller as Creature;
            var _target = defRoll.Fulfiller as Creature;
            var _ovrQual = new Qualifier(_pusher, typeof(Overrun), _target);

            var _check = Deltable.GetCheckNotify(_pusher?.ID, @"Overrun", _target.ID, @"Overrun Defend");
            while (true)
            {
                // attacker result
                _check.CheckInfo.Deltas.Clear();
                var _atkCheck =
                    (_pusher.Abilities.Strength.CheckValue(_ovrQual, atkRoll.RollValue, _check.CheckInfo) ?? 0)
                    + _pusher.BodyDock.OpposedModifier.Value;
                var _pusherOM = _pusher.BodyDock.OpposedModifier.Value;
                if (_pusherOM != 0)
                {
                    _atkCheck += _pusherOM;
                    _check.CheckInfo.AddDelta(_pusher.BodyDock.OpposedModifier.Name, _pusherOM);
                    _check.CheckInfo.Result += _pusherOM;
                }

                // defender result
                var _strCheck = new DeltaCalcInfo(_target.ID, @"Overrun Defend");
                var _dexCheck = new DeltaCalcInfo(_target.ID, @"Overrun Defend");
                var _defCheck = Math.Max(
                    _target.Abilities.Strength.CheckValue(_ovrQual, defRoll.RollValue, _strCheck) ?? 0,
                    _target.Abilities.Dexterity.CheckValue(_ovrQual, defRoll.RollValue, _dexCheck) ?? 0);
                _check.OpposedInfo = (_dexCheck.Result > _strCheck.Result)
                    ? _dexCheck
                    : _strCheck;
                var _targetOM = _target.BodyDock.OpposedModifier.Value;
                if (_targetOM != 0)
                {
                    _check.OpposedInfo.AddDelta(_target.BodyDock.OpposedModifier.Name, _targetOM);
                    _check.OpposedInfo.Result += _targetOM;
                }

                // if defender is helpless, this always succeeds
                if ((_atkCheck > _check.OpposedInfo.Result) || _target.Conditions.Contains(Condition.Helpless))
                {
                    // attacker wins
                    var _prone = new ProneEffect(typeof(Overrun));
                    _target.AddAdjunct(_prone);
                    overrunStep.EnqueueNotify(new BadNewsNotify(_target.ID, @"Overrun", new Info { Message = @"Overrun and Prone" }), _target.ID);
                    overrunStep.EnqueueNotify(new RefreshNotify(true, true, false, false, false), _target.ID);
                    overrunStep.EnqueueNotify(new GoodNewsNotify(_pusher.ID, @"Overrun", new Info { Message = @"Overrun Opponent Prone" }), _pusher.ID);
                    overrunStep.EnqueueNotify(new RefreshNotify(true, false, false, false, false), _pusher.ID);
                    return;
                }
                else if (_atkCheck < _defCheck)
                {
                    // defender wins
                    if (canCounter)
                    {
                        // block further movement of pusher
                        var _activity = overrunStep.Process as CoreActivity;
                        overrunStep.AppendFollowing(new CanStillMoveStep(_activity,
                            (_activity.Action as MovementAction)?.MovementBudget));

                        // and counter
                        overrunStep.AppendFollowing(new CounterOverrun(overrunStep, _pusher, _target));
                    }
                    return;
                }

                // dreaded tie...rather than going back to players, just do a run-off
                atkRoll.RollValue = atkRoll.Roller.RollValue(_pusher.ID, @"Overrun", @"Tie breaker", _pusher.ID);
                defRoll.RollValue = defRoll.Roller.RollValue(_target.ID, @"Overrun", @"Tie breaker", _target.ID);
            }
        }
        #endregion
    }
}
