using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public class DispelRepelCreatures : DriveCreaturePowerDef, IGeneralSubMode, IApplyPowerCapable<SuperNaturalPowerActionSource>
    {
        public DispelRepelCreatures(int powerLevel, PowerBattery battery, ICreatureFilter filter)
            : base(powerLevel, battery, filter)
        {
        }

        #region public override CoreStep CreateStep(PowerBurstCapture<SuperNaturalPowerSource> burst, int powerLevel, int critterPowerLevel, AimTarget target)
        public override CoreStep CreateStep(PowerBurstCapture<SuperNaturalPowerActionSource> burst, int powerLevel,
            int critterPowerLevel, AimTarget target)
        {
            if (target.Target is Creature _critter)
            {
                // attempt to remove repulsion effects
                var _checkRoll = burst.Context.FirstOrDefault(_t => _t.Key.Equals(@"Roll.CheckValue")) as ValueTarget<int>;
                if (_critter.Adjuncts.OfType<RepulsedEffect>().Any(_repulsed => _checkRoll.Value >= _repulsed.CheckValue))
                {
                    var _step = DeliverNextStep(burst.Activation, 0, target, true, 0);
                    AddCheckValue(_step, burst);
                    return _step;
                }
            }
            return null;
        }
        #endregion

        public override string DisplayName => $@"Dispel {CreatureFilter.Description} repulsion";
        public override string Description => $@"Dispel {CreatureFilter.Description} repulsion";

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
            if ((step.DeliveryInteraction.Target is Creature _critter) 
                && (step.DeliveryInteraction.InteractData is PowerActionTransit<SuperNaturalPowerActionSource> _transit ))
            {
                var _check = _transit.AllTargets.FirstOrDefault(_t => _t.Key.Equals(@"Roll.CheckValue")) as ValueTarget<int>;

                // remove repulsion effects of lesser or equal check value
                foreach (var _repulsion in (from _rep in _critter.Adjuncts.OfType<RepulsedEffect>()
                                            where (_check != null ? _check.Value : 0) >= _rep.CheckValue
                                            select _rep).ToList())
                {
                    // force it out...at the magic power that "owns" it
                    _repulsion.MagicPowerEffect.Eject();
                }
            }
        }

        #endregion
    }
}
