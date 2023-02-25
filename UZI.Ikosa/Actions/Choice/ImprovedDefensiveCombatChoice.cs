using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Improved defensive combat choice: 0..-5 ATK, 0..+5 Armor Rating for up to start of next turn</summary>
    [Serializable]
    public class ImprovedDefensiveCombatChoice : ActionBase
    {
        #region public ImprovedDefensiveCombatChoice()
        /// <summary>Improved defensive combat choice: 0..-5 ATK, 0..+5 Armor Rating for up to start of next turn</summary>
        public ImprovedDefensiveCombatChoice(IActionSource source)
            : base(source, new ActionTime(TimeType.Regular),
            new ActionTime(TimeType.FreeOnTurn), false, false, @"200")
        {
        }
        #endregion

        public override string Key => @"ImprovedDefensiveCombat";
        public override string DisplayName(CoreActor actor) => @"Improved Defensive Combat: 0..-5 ATK, 0..+5 AR for up to 1 round";
        public override bool IsMental => true;
        public override bool IsChoice => true;
        public override bool IsStackBase(CoreActivity activity) => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => null;

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // do everything in-action (no steps, no prerequisites)
            if (_Budget != null)
            {
                var _dial = (activity.Targets[0] as OptionTarget).Option as OptionAimValue<int>;
                var _current = (from _bi in _Budget.BudgetItems
                                where _bi.Key == Source
                                let _dcb = _bi.Value as DefensiveCombatBudget
                                where _dcb != null
                                select _dcb).FirstOrDefault();

                if ((_current != null) && (_dial.Value != _current.ArmorRating.Value) && !_current.IsLocked)
                {
                    // current budget not the same as dialed in value, clear it
                    _Budget.BudgetItems.Remove(Source);
                    _current = null;
                }

                if ((_dial.Value > 0) && (_current == null))
                {
                    // need to set up a new budget since there is a value, but no current budget
                    var _defensive = new DefensiveCombatBudget(Source, true, 0 - _dial.Value, _dial.Value, @"Defensive Combat");
                    _Budget.BudgetItems.Add(_defensive.Source, _defensive);
                }

                _Budget.Choices[Key] = new ChoiceBinder(this, _Budget.Actor, false, _dial);

                return activity.GetActivityResultNotifyStep($@"Set to {_dial.Value}");
            }

            return activity.GetActivityResultNotifyStep(@"No current action budget to set");
        }
        #endregion

        #region private IEnumerable<OptionAimOption> Options()
        private IEnumerable<OptionAimOption> Options()
        {
            var _current = (_Budget != null)
                ? (from _bi in _Budget.BudgetItems
                   where (_bi.Value is DefensiveCombatBudget) && (_bi.Key == Source)
                   select _bi.Value as DefensiveCombatBudget).FirstOrDefault()?.Attack.Value
                : 0;

            yield return new OptionAimValue<int>
            {
                Key = @"Off",
                Name = @"Off",
                Description = @"Off",
                Value = 0,
                IsCurrent = (_current == 0)
            };
            if (_Budget?.Actor is Creature _critter)
            {
                for (var _dx = 1; (_dx <= 5) && (_dx <= _critter.BaseAttack.EffectiveValue); _dx++)
                {
                    yield return new OptionAimValue<int>
                    {
                        Key = _dx.ToString(),
                        Name = $@"-{_dx} ATK, +{_dx} Armor Rating",
                        Description = $@"-{_dx} ATK, +{_dx} Armor Rating",
                        Value = _dx,
                        IsCurrent = (_current == _dx)
                    };
                }
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
