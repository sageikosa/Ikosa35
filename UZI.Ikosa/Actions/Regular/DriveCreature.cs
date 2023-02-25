using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Fidelity;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Interactions;
using Uzi.Visualize;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Action for repelling, overwhelming, reinforcing and dispelling repel of creatures [ActionBase]</summary>
    [Serializable]
    public class DriveCreature : SuperNaturalPowerUse
    {
        #region construction
        public DriveCreature(SuperNaturalPowerActionSource source, string orderKey)
            : base(source, orderKey)
        {
        }

        public DriveCreature(SuperNaturalPowerActionSource source, ActionTime actionTime, string orderKey)
            : base(source, actionTime, orderKey)
        {
        }
        #endregion

        #region state
        private PowerAffectTracker _Tracker;
        #endregion

        public override bool IsHarmless => false;
        public override bool CombatList => true;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Drive Creature", activity.Actor, observer);

        protected override CoreStep OnPerformActivity(CoreActivity activity)
            => new DriveCreatureDeliveryStep(activity, activity.Action as PowerUse<SuperNaturalPowerActionSource>, activity.Actor);

        #region public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new PersonalStartAim(@"Self", @"Self", activity.Actor);
            yield return new OptionAim(@"SkipAffected", @"Skip Effected Creatures", true, FixedRange.One, FixedRange.One, SkipOptions());
            yield break;
        }
        #endregion

        #region public IEnumerable<OptionAimOption> SkipOptions()
        public IEnumerable<OptionAimOption> SkipOptions()
        {
            yield return new OptionAimOption
            {
                Key = @"Skip",
                Name = @"Skip",
                Description = @"Creatures already driven are skipped and do not use driving capacity"
            };
            yield return new OptionAimOption
            {
                Key = @"Affect",
                Name = @"Affect",
                Description = @"Creatures currently driven are affected and use driving capacity"
            };
            yield break;
        }
        #endregion

        #region public override void Deliver(PowerDeliveryStep<SuperNaturalPowerSource> step)
        public override void ActivatePower(PowerActivationStep<SuperNaturalPowerActionSource> step)
        {
            // get burst geometry
            var _target = step.TargetingProcess.Targets[0] as LocationTarget;
            var _source = PowerActionSource;
            var _sphere = new Geometry(new SphereBuilder(Convert.ToInt32(_source.SuperNaturalPowerActionDef.GetCapability<IRegionCapable>().Dimensions(step.Actor, _source.PowerLevel).FirstOrDefault() / 5)), new Intersection(_target.Location), true);

            // initial transit ensures the ability can be activated
            PowerActionDef<SuperNaturalPowerActionSource>.DeliverBurstToMultipleSteps(step,
                new Intersection(_target.Location), _sphere, null);
        }
        #endregion

        #region public override void Apply(PowerApplyStep<SuperNaturalPowerSource> step)
        public override void ApplyPower(PowerApplyStep<SuperNaturalPowerActionSource> step)
        {
            if (step.DeliveryInteraction.InteractData is MagicPowerEffectTransit<SuperNaturalPowerActionSource>)
            {
                // if the delivery involved delivering effects, deliver them now
                // NOTE: repulse, overwhelm, reinforce
                SuperNaturalPowerActionDef.ApplySuperNaturalEffect(step);
            }
            else
            {
                // otherwise, apply directly (GeneralSubMode-0)
                // NOTE: destroy, command, dispel-repulse
                var _apply = CapabilityRoot.GetCapability<IApplyPowerCapable<SuperNaturalPowerActionSource>>();
                _apply.ApplyPower(step);
            }
        }
        #endregion

        public static bool IsSkipping(PowerActivationStep<SuperNaturalPowerActionSource> delivery)
            => (delivery.TargetingProcess.Targets.FirstOrDefault(_t => _t.Key.Equals(@"SkipAffected")) is OptionTarget _skipOpt)
            ? _skipOpt.Option.Key.Equals(@"Skip")
            : false;

        public override int StandardClassPowerLevel
            => PowerActionSource.PowerClass.ClassPowerLevel.QualifiedValue(
                new Interaction(null, null, null, 
                    new DriveCreatureData(null, PowerActionSource.SuperNaturalPowerActionDef as IDriveCreaturePowerDef)));

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override PowerAffectTracker PowerTracker
            => _Tracker ??= new PowerAffectTracker();
    }
}
