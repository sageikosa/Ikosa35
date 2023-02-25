using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class DoubleWeaponTwoHandedChoice : ActionBase
    {
        public DoubleWeaponTwoHandedChoice(DoubleMeleeWeaponBase weapon)
            : base(weapon, new ActionTime(TimeType.FreeOnTurn), false, false, @"402")
        {
        }

        public DoubleMeleeWeaponBase Weapon => Source as DoubleMeleeWeaponBase;
        public override string Key => @"DoubleWeapon.UseAsTwoHanded";
        public override string DisplayName(CoreActor actor) => @"Switch between two-handed and double weapon fighting";
        public override bool IsMental => true;
        public override bool IsChoice => true;
        public override bool IsStackBase(CoreActivity activity) => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            return null;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _switch = (activity.Targets[0] as OptionTarget).Option;
            var _two = (_switch != null) && _switch.Key.Equals(@"Two-Handed", StringComparison.OrdinalIgnoreCase);
            Weapon.UseAsTwoHanded = _two;
            _Budget.Choices[string.Format(@"{0}.{1}", Weapon.ID, Key)] = new ChoiceBinder(this, _Budget.Actor, false, _switch);

            // status step
            return activity.GetActivityResultNotifyStep(_switch.Name);
        }

        #region private IEnumerable<OptionAimOption> Options()
        private IEnumerable<OptionAimOption> Options()
        {
            if (Weapon.UseAsTwoHanded)
            {
                yield return new OptionAimOption
                {
                    Key = @"Two-Handed",
                    Name = @"Two-Handed",
                    Description = @"Use weapon as a two-handed weapon with only the main head active",
                    IsCurrent = true
                };
                yield return new OptionAimOption
                {
                    Key = @"Double",
                    Name = @"Double",
                    Description = @"Use weapon as a double weapon, allowing two-weapon fighting with both heads"
                };
            }
            else
            {
                yield return new OptionAimOption
                {
                    Key = @"Double",
                    Name = @"Double",
                    Description = @"Use weapon as a double weapon, allowing two-weapon fighting with both heads",
                    IsCurrent = true
                };
                yield return new OptionAimOption
                {
                    Key = @"Two-Handed",
                    Name = @"Two-Handed",
                    Description = @"Use weapon as a two-handed weapon with only the main head active"
                };
            }
            yield break;
        }
        #endregion

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new OptionAim(@"Handedness", @"Use Two-Handed or Double", true, FixedRange.One, FixedRange.One, Options());
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}