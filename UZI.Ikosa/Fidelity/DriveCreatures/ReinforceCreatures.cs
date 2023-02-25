using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Tactical;
using Uzi.Core;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public class ReinforceCreatures : DriveCreaturePowerDef, IDurableCapable
    {
        public ReinforceCreatures(int powerLevel, PowerBattery battery, ICreatureFilter filter)
            : base(powerLevel, battery, filter)
        {
        }

        #region public override CoreStep CreateStep(PowerBurstCapture<SuperNaturalPowerSource> burst, int powerLevel, int critterPowerLevel, AimTarget target)
        public override CoreStep CreateStep(PowerBurstCapture<SuperNaturalPowerActionSource> burst, int powerLevel,
            int critterPowerLevel, AimTarget target)
        {
            if (target.Target is Creature _critter)
            {
                // NOTE: determine reinforcement on *Qualified* PowerLevel, but calculate on *Effective* PowerLevel
                var _max = burst.Context.FirstOrDefault(_t => _t.Key.Equals(@"Roll.MaxPowerDie")) as ValueTarget<int>;
                if (_max.Value > critterPowerLevel)
                {
                    // reinforcing power level greater than creature's power level
                    // attempt to apply cowering
                    var _actor = burst.Activation.Actor;
                    var _aLoc = _actor.GetLocated()?.Locator;
                    var _step = DeliverDurableNextStep(burst.Activation, _actor, _aLoc, _aLoc.PlanarPresence,
                        burst.Activation.PowerUse, burst.Activation.TargetingProcess.Targets, target, true, 0);
                    if (_step is PowerApplyStep<SuperNaturalPowerActionSource>)
                    {
                        // use effective value of powerlevel (rather than qualified value) to avoid other reinforcing deltas
                        var _applyStep = _step as PowerApplyStep<SuperNaturalPowerActionSource>;
                        var _deltaVal = new ValueTarget<int>(@"Reinforce.Delta", _max.Value - _critter.AdvancementLog.PowerLevel.EffectiveValue);

                        // durable effect or direct power transit?
                        // ... determines where additional targets are parked
                        if (_applyStep.DeliveryInteraction.InteractData is MagicPowerEffectTransit<SuperNaturalPowerActionSource> _transit)
                        {
                            // add check values to magic effects (for Activation)
                            foreach (var _durable in _transit.MagicPowerEffects.OfType<DurableMagicEffect>())
                                _durable.AllTargets.Add(_deltaVal);
                        }
                    }
                    return _step;
                }
            }
            return null;
        }
        #endregion

        public override string DisplayName
            => $@"Reinforce {CreatureFilter.Description} against repulsion";

        public override string Description
            => $@"Reinforce {CreatureFilter.Description} against repulsion";

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
            var _critter = target as Creature;
            if ((source is MagicPowerEffect _magicEffect) && (_critter != null))
            {
                // add reinforcement to power level
                var _deltaVal = _magicEffect.GetTargetValue<int>(@"Reinforce.Delta", 0);
                var _delta = new ReinforceCreatureDelta(_deltaVal);
                _critter.AdvancementLog.PowerLevel.Deltas.Add(_delta);
                return _delta;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
            => (source.ActiveAdjunctObject as ReinforceCreatureDelta)?.DoTerminate();

        public bool IsDismissable(int subMode) => false;
        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode) => string.Empty;
        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact) { yield break; }

        public DurationRule DurationRule(int subMode)
        {
            // always 10 rounds
            return new DurationRule(DurationType.Span, new SpanRulePart(10, new Round()));
        }

        #endregion
    }

    [Serializable]
    public class ReinforceCreatureDelta : IQualifyDelta
    {
        #region construction
        public ReinforceCreatureDelta(int delta)
        {
            _Term = new TerminateController(this);
            _Boost = new QualifyingDelta(delta, typeof(ReinforceCreatures), @"Reinforcement");
        }
        #endregion

        #region data
        private TerminateController _Term;
        private IDelta _Boost;
        #endregion

        #region ISupplyQualifyDelta Members

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            // when checking against a creature driving interaction, provide the boost
            if (!(qualify is Interaction _iAct))
                yield break;
            if (_iAct.InteractData is DriveCreatureData)
            {
                yield return _Boost;
            }
            yield break;
        }

        #endregion

        #region IControlTerminate Members

        public void DoTerminate()
        {
            _Term.DoTerminate();
        }

        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Term.TerminateSubscriberCount;

        #endregion
    }
}
