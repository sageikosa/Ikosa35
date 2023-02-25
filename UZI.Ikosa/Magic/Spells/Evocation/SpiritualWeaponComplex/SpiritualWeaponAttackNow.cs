using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class SpiritualWeaponAttackNow : ActionBase
    {
        public SpiritualWeaponAttackNow(SpiritualWeaponGroup spiritualWeaponGroup, string orderKey)
            : base(spiritualWeaponGroup.SpellSource, new ActionTime(Contracts.TimeType.FreeOnTurn), false, false, orderKey)
        {
            _Group = spiritualWeaponGroup;
        }

        #region state
        private SpiritualWeaponGroup _Group;
        #endregion

        public override string Key => @"SpiritualWeapon.AttackNow";
        public SpiritualWeaponGroup SpiritualWeaponGroup => _Group;

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
            => Enumerable.Empty<AimingMode>();

        public override string DisplayName(CoreActor observer)
            => observer == _Group.ControlCreature
                ? @"Allow Spiritual Weapon to attack now"
                : string.Empty;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Allowing next attack", activity.Actor, observer);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
            => new SimpleStep(activity, SpiritualWeaponGroup.Weapon);
    }
}
