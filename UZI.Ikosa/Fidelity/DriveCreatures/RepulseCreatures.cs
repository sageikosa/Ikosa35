using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Tactical;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public class RepulseCreatures : DriveCreaturePowerDef, IDurableCapable, IApplyPowerCapable<SuperNaturalPowerActionSource>, IGeneralSubMode
    {
        public RepulseCreatures(int powerLevel, PowerBattery battery, ICreatureFilter filter)
            : base(powerLevel, battery, filter)
        {
        }

        #region public override CoreStep CreateStep(PowerBurstCapture<SuperNaturalPowerSource> burst, int powerLevel, int critterPowerLevel, AimTarget target)
        public override CoreStep CreateStep(PowerBurstCapture<SuperNaturalPowerActionSource> burst, int powerLevel,
            int critterPowerLevel, AimTarget target)
        {
            if (powerLevel >= (critterPowerLevel * 2))
            {
                // double overpowering == destroy
                return DeliverNextStep(burst.Activation, 0, target, true, 0);
            }
            else
            {
                if ((target.Target is Creature _critter)
                    && _critter.Conditions.Contains(Condition.Repulsed))
                {
                    if (DriveCreature.IsSkipping(burst.Activation))
                    {
                        return null;
                    }
                }

                // otherwise, repulse
                // attempt to apply cowering
                var _actor = burst.Activation.Actor;
                var _aLoc = _actor.GetLocated()?.Locator;
                var _step = DeliverDurableNextStep(burst.Activation, _actor, _aLoc, _aLoc.PlanarPresence,
                    burst.Activation.PowerUse, burst.Activation.TargetingProcess.Targets, target, true, 0);
                AddCheckValue(_step, burst);
                return _step;
            }
        }
        #endregion

        public override string DisplayName => $@"Repulse {CreatureFilter.Description}";
        public override string Description => $@"Repulse or destroy {CreatureFilter.Description}";

        #region IDurableMode Members

        #region public IEnumerable<int> DurableSubModes { get; }
        public IEnumerable<int> DurableSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }
        #endregion

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            var _magicEffect = source as MagicPowerEffect;
            if (_magicEffect != null)
            {
                var _check = _magicEffect.GetTargetValue<int>(@"Roll.CheckValue", 0);
                var _repulse = new RepulsedEffect(_magicEffect, _check);
                target.AddAdjunct(_repulse);
                return _repulse;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            target.RemoveAdjunct((RepulsedEffect)source.ActiveAdjunctObject);
        }

        public bool IsDismissable(int subMode) { return false; }
        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode) { return string.Empty; }
        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact) { yield break; }

        public DurationRule DurationRule(int subMode)
        {
            // always 10 rounds
            return new DurationRule(DurationType.Span, new SpanRulePart(10, new Round()));
        }

        #endregion

        #region IGeneralSubMode Members

        #region public IEnumerable<int> GeneralSubModes
        public IEnumerable<int> GeneralSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }
        #endregion

        public string GeneralSaveKey(CoreTargetingProcess targetProcess, Interaction workSet, int subMode) { return null; }
        public IEnumerable<StepPrerequisite> GetGeneralSubModePrerequisites(int subMode, Interaction interact) { yield break; }

        #endregion

        #region IApplyPowerMode<SuperNaturalPowerSource> Members

        public void ApplyPower(PowerApplyStep<SuperNaturalPowerActionSource> step)
        {
            if (step.DeliveryInteraction.Target is Creature _critter)
            {
                var _dead = new DeadEffect(typeof(RepulseCreatures), (_critter.Setting as ITacticalMap)?.CurrentTime ?? 0);
                _critter.AddAdjunct(_dead);
            }
        }

        #endregion
    }

    [Serializable]
    public class DestroyCreatures : RepulseCreatures
    {
        public DestroyCreatures(int powerLevel, PowerBattery battery, ICreatureFilter filter)
            : base(powerLevel, battery, filter)
        {
        }

        public override CoreStep CreateStep(PowerBurstCapture<SuperNaturalPowerActionSource> burst, int powerLevel, int critterPowerLevel, AimTarget target)
        {
            // destroy only
            return DeliverNextStep(burst.Activation, 0, target, true, 0);
        }

        public override string DisplayName { get { return string.Format(@"Destroy {0}", CreatureFilter.Description); } }

        public override string Description
        {
            get { return string.Format(@"Destroy {0}", CreatureFilter.Description); }
        }
    }
}
