using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class DoubleWeaponMainHeadChoice : ActionBase
    {
        public DoubleWeaponMainHeadChoice(DoubleMeleeWeaponBase weapon)
            : base(weapon, new ActionTime(TimeType.FreeOnTurn), false, false, @"401")
        {
        }

        public DoubleMeleeWeaponBase Weapon => Source as DoubleMeleeWeaponBase;
        public override string Key => @"DoubleWeapon.MainHead";
        public override string DisplayName(CoreActor actor) => @"Switch main head for the double weapon";
        public override bool IsMental => true;
        public override bool IsChoice => true;

        public override bool IsStackBase(CoreActivity activity)
        {
            return false;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            return null;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if ((activity.Targets[0] as OptionTarget).Option is OptionAimValue<int> _switch)
            {
                Weapon.MainHeadIndex = _switch.Value;
                _Budget.Choices[string.Format(@"{0}.{1}", Weapon.ID, Key)] = new ChoiceBinder(this, _Budget.Actor, false, _switch);

                // status step
                return activity.GetActivityResultNotifyStep(_switch.Name);
            }

            // status step
            return activity.GetActivityResultNotifyStep(@"No switch setting");
        }

        #region private IEnumerable<OptionAimOption> Options()
        private IEnumerable<OptionAimOption> Options()
        {
            var _mainHead = Weapon.Head[Weapon.MainHeadIndex];
            yield return new OptionAimValue<int>
            {
                Key = Weapon.MainHeadIndex.ToString(),
                Name = string.Format(@"{0} ({1}/{2}) {3}",
                                    _mainHead.CurrentDamageRollString, _mainHead.CriticalLow,
                                    _mainHead.CriticalMultiplier, string.Join(@"|", (from _dt in _mainHead.DamageTypes
                                                                                     select _dt.ToString()).ToArray())),
                Description = _mainHead.Name,
                Value = Weapon.MainHeadIndex,
                IsCurrent = true
                // TODO: Magical/extra-damages/other-adjuncts
            };
            for (var _hx = 0; _hx < Weapon.HeadCount; _hx++)
            {
                var _head = Weapon.Head[_hx];
                if (_hx != Weapon.MainHeadIndex)
                    yield return new OptionAimValue<int>
                    {
                        Key = _hx.ToString(),
                        Name = string.Format(@"{0} ({1}/{2}) {3}",
                                            _head.CurrentDamageRollString, _head.CriticalLow,
                                            _head.CriticalMultiplier, string.Join(@"|", (from _dt in _head.DamageTypes
                                                                                         select _dt.ToString()).ToArray())),
                        Description = _head.Name,
                        Value = _hx
                        // TODO: Magical/extra-damages/other-adjuncts
                    };
            }
            yield break;
        }
        #endregion

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new OptionAim(@"MainHead", @"Switch Main Head to use for attacks", true, FixedRange.One, FixedRange.One, Options());
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
