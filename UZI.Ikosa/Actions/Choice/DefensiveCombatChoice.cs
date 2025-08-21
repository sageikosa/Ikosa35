using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Standard defensive combat choice: -4 ATK, +2 Armor Rating for up to start of next turn</summary>
    [Serializable]
    public class DefensiveCombatChoice : ActionBase
    {
        #region public DefensiveCombatChoice()
        /// <summary>Standard defensive combat choice: -4 ATK, +2 Armor Rating for up to start of next turn</summary>
        public DefensiveCombatChoice(IActionSource source)
            : base(source, new ActionTime(TimeType.Regular),
                   new ActionTime(TimeType.FreeOnTurn), false, false, @"200")
        {
        }
        #endregion

        public override string Key => @"DefensiveCombat";
        public override string DisplayName(CoreActor actor) => @"Defensive Combat: -4 ATK, +2 AR for up to 1 round";
        public override bool IsStackBase(CoreActivity activity) => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => null;

        public override bool IsMental => false;
        public override bool IsChoice => true;

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if (_Budget != null)
            {
                var _switch = (activity.Targets[0] as OptionTarget).Option;
                var _on = (_switch != null) && _switch.Key.Equals(@"On", StringComparison.OrdinalIgnoreCase);
                var _current = (from _bi in _Budget.BudgetItems
                                where (_bi.Value is DefensiveCombatBudget)
                                && ((_bi.Key as Type) == typeof(DefensiveCombatChoice))
                                select _bi.Value as DefensiveCombatBudget).FirstOrDefault();

                // do everything in-action (no steps, no prerequisites)
                if (_on && (_current == null))
                {
                    var _defensive = new DefensiveCombatBudget(typeof(DefensiveCombatChoice), true, -4, 2, @"Defensive Combat");
                    _Budget.BudgetItems.Add(_defensive.Source, _defensive);
                }
                else if (!_on && (_current != null) && !_current.IsLocked)
                {
                    _Budget.BudgetItems.Remove(_current.Source);
                }
                _Budget.Choices[Key] = new ChoiceBinder(this, _Budget.Actor, false, _switch);

                // status step
                return activity.GetActivityResultNotifyStep($@"Set to {_switch.Name}");
            }

            // status step
            return activity.GetActivityResultNotifyStep(@"No current action budget to set");
        }
        #endregion

        #region private IEnumerable<OptionAimOption> Options()
        private IEnumerable<OptionAimOption> Options()
        {
            DefensiveCombatBudget _current = null;
            if (_Budget != null)
            {
                _current = (from _bi in _Budget.BudgetItems
                            where (_bi.Value is DefensiveCombatBudget)
                            && ((_bi.Key as Type) == typeof(DefensiveCombatChoice))
                            select _bi.Value as DefensiveCombatBudget).FirstOrDefault();
            }

            if (_current != null)
            {
                yield return new OptionAimOption
                {
                    Key = @"On",
                    Name = @"On",
                    Description = @"-4 ATK, +2 Armor Rating",
                    IsCurrent = true
                };
                yield return new OptionAimOption
                {
                    Key = @"Off",
                    Name = @"Off",
                    Description = @"Off"
                };
            }
            else
            {
                yield return new OptionAimOption
                {
                    Key = @"Off",
                    Name = @"Off",
                    Description = @"Off",
                    IsCurrent = true
                };
                yield return new OptionAimOption
                {
                    Key = @"On",
                    Name = @"On",
                    Description = @"-4 ATK, +2 Armor Rating"
                };
            }
            yield break;
        }
        #endregion

        #region public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new OptionAim(@"DefensiveCombat", @"Standard Defensive Combat", true, FixedRange.One, FixedRange.One, Options());
            yield break;
        }
        #endregion

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
