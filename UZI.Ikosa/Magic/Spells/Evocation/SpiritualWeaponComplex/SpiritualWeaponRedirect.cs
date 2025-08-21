using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class SpiritualWeaponRedirect : ActionBase, ISimpleStep, IPowerUse<SpellSource>, IFinalizeProcess
    {
        public SpiritualWeaponRedirect(SpiritualWeaponGroup spiritualWeaponGroup, ActionTime cost, string orderKey)
            : base(spiritualWeaponGroup.SpellSource, cost, false, false, orderKey)
        {
            _Group = spiritualWeaponGroup;
        }

        #region state
        private SpiritualWeaponGroup _Group;
        #endregion

        public override string Key => @"SpiritualWeapon.Redirect";
        public override bool IsMental => true;

        public SpiritualWeaponGroup SpiritualWeaponGroup => _Group;
        public SpellSource PowerActionSource => SpellSource;

        public SpellSource SpellSource
            => ActionSource as SpellSource;

        public ICapabilityRoot CapabilityRoot
            => SpiritualWeaponGroup.Weapon.MagicPowerEffect.CapabilityRoot;

        public PowerAffectTracker PowerTracker
            => SpiritualWeaponGroup.Weapon.MagicPowerEffect.PowerTracker;

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            var _mode = SpellSource.SpellDef.SpellModes.FirstOrDefault();
            if (_mode != null)
            {
                return _mode.AimingMode(activity.Actor, _mode);
            }
            return Enumerable.Empty<AimingMode>(); ;
        }

        public override string DisplayName(CoreActor observer)
        {
            if (observer == SpiritualWeaponGroup.ControlCreature)
            {
                return $@"Redirect: {SpellSource.SpellDef.SpellModes.FirstOrDefault()?.DisplayName}";
            }
            else
            {
                return $@"Nothing";
            }
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            if (observer == activity.Actor)
            {
                return ObservedActivityInfoFactory.CreateInfo(DisplayName(observer), activity.Actor, observer);
            }
            else
            {
                return null;
            }
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity.EnqueueRegisterPreEmptively(Budget);
            return new SimpleStep(activity, this);
        }

        public bool DoStep(CoreStep actualStep)
        {
            // max attacks = 1, when target switched
            SpiritualWeaponGroup.Weapon.MaxAttacks = 1;

            // remove spiritual weapon target
            SpiritualWeaponGroup.Target.Eject();

            // deliver durable
            var _actor = SpiritualWeaponGroup.ControlCreature;
            var _aLoc = _actor?.GetLocated()?.Locator;
            var _tProc = actualStep.Process as CoreTargetingProcess;
            var _targets = _tProc?.Targets ?? [];
            _tProc.AddFinalizer(this);
            var _aim = _tProc?.GetFirstTarget<AwarenessTarget>(@"Creature");
            SpellDef.DeliverDurableNextStep(actualStep, _actor, _aLoc, PlanarPresence.Material, this, _targets, _aim, false, 1);
            return true;
        }

        public void ActivatePower(PowerActivationStep<SpellSource> step)
        {
            // NOP
        }

        public void ApplyPower(PowerApplyStep<SpellSource> step)
        {
            var _tProc = step.TargetingProcess;
            var _target = _tProc.GetFirstTarget<AwarenessTarget>(@"Creature")?.Target as Creature;
            _target.AddAdjunct(new SpiritualWeaponTarget(SpellSource, SpiritualWeaponGroup));
        }

        public void FinalizeProcess(CoreProcess process, bool deactivated)
        {
            if (deactivated)
            {
                SpiritualWeaponGroup.Weapon.Eject();
            }
        }
    }
}
