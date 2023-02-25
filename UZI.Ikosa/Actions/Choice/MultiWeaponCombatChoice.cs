using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class MultiWeaponCombatChoice : ActionBase
    {
        public MultiWeaponCombatChoice(IActionSource source)
            : base(source, new ActionTime(TimeType.Regular), new ActionTime(TimeType.FreeOnTurn), false, false, @"200")
        {
            _Budget = null;
        }

        public override string Key => @"MultiWeaponCombat";
        public override string DisplayName(CoreActor actor) 
            => @"Multi-Weapon Combat: apply multi-weapon penalties on attacks to get 1 extra attack during full attack finisher";
        public override bool IsMental => true;
        public override bool IsChoice => true;
        public override bool IsStackBase(CoreActivity activity) => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => null;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if (_Budget != null)
            {
                var _critter = _Budget.Actor as Creature;
                if (_critter == null)
                {
                    // do everything in-action (no steps, no prerequisites)
                    var _switch = (activity.Targets[0] as OptionTarget).Option;
                    var _on = (_switch != null) && _switch.Key.Equals(@"On", StringComparison.OrdinalIgnoreCase);

                    var _current = _Budget.Actor.Adjuncts.OfType<OffHandBudgetFactory>()
                        .FirstOrDefault(_f => (_f.Source as Type) == typeof(MultiWeaponCombatChoice));

                    if (_on && (_current == null))
                    {
                        // factory to create off-hand budgets when full attack is made
                        _Budget.Actor.AddAdjunct(new OffHandBudgetFactory(typeof(MultiWeaponCombatChoice)));
                    }
                    else if (!_on && (_current != null))
                    {
                        // remove off-hand budget factory
                        _current.Eject();
                    }
                    _Budget.Choices[Key] = new ChoiceBinder(this, _Budget.Actor, false, _switch);

                    // status step
                    return activity.GetActivityResultNotifyStep($@"Set to {_switch.Name}");
                }
            }

            // status step
            return activity.GetActivityResultNotifyStep(@"No current action budget or invalid creature");
        }

        private IEnumerable<OptionAimOption> Options()
        {
            if ((_Budget != null) && _Budget.Actor.HasActiveAdjunct<OffHandBudgetFactory>())
            {
                yield return new OptionAimOption { Key = @"On", Name = @"On", Description = @"On", IsCurrent = true };
                yield return new OptionAimOption { Key = @"Off", Name = @"Off", Description = @"Off" };
            }
            else
            {
                yield return new OptionAimOption { Key = @"Off", Name = @"Off", Description = @"Off", IsCurrent = true };
                yield return new OptionAimOption { Key = @"On", Name = @"On", Description = @"On" };
            }
            yield break;
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new OptionAim(@"MultiWeaponCombat", @"Multi-Weapon Combat", true, FixedRange.One, FixedRange.One, Options());
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
