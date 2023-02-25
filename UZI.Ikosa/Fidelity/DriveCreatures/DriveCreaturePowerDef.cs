using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Tactical;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public abstract class DriveCreaturePowerDef : SuperNaturalPowerActionDef, IDriveCreaturePowerDef, IRegionCapable, IBurstCaptureCapable
    {
        public DriveCreaturePowerDef(int powerLevel, PowerBattery battery, ICreatureFilter filter)
            : base(battery)
        {
            _Filter = filter;
            _PowerLevel = powerLevel;
        }

        #region private data
        private ICreatureFilter _Filter;
        private int _PowerLevel;
        #endregion

        public override MagicStyle MagicStyle => new Evocation();
        public ICreatureFilter CreatureFilter => _Filter;

        /// <summary>Comparative level of the power</summary>
        public override int PowerLevel => _PowerLevel;

        /// <summary>Reflexively returns itself, derived wrappers will return wrapped instances</summary>
        public override SuperNaturalPowerActionDef SeedPowerDef => this;

        public override MagicPowerDefInfo ToMagicPowerDefInfo()
            => this.GetMagicPowerDefInfo();

        public override PowerDefInfo ToPowerDefInfo()
            => ToMagicPowerDefInfo();

        #region IRegionMode Members

        public IEnumerable<double> Dimensions(CoreActor actor, int powerLevel)
        {
            yield return 60d;
            yield break;
        }

        #endregion

        #region IBurstCapture Members

        #region public void PostInitialize(BurstCapture burst)
        public void PostInitialize(BurstCapture burst)
        {
            // get the burst as a super natural burst
            if (burst is PowerBurstCapture<SuperNaturalPowerActionSource> _suBurst)
            {
                // setup interaction for qualified checks
                var _creature = _suBurst.Activation.Actor as Creature;
                var _driveData = new DriveCreatureData(_creature, this);
                var _driveSet = new Interaction(_creature, _suBurst.Source, null, _driveData);

                // get shared qualified values
                var _infLevel = _suBurst.PowerActionSource.PowerClass.ClassPowerLevel.QualifiedValue(_driveSet);
                var _charisma = _creature.Abilities.Charisma.QualifiedDeltas(_driveSet).Sum(_d => _d.Value);

                // track max for easy calculation...
                if (_suBurst.Activation.AllPrerequisites<RollPrerequisite>(@"Roll.MaxPowerDie").FirstOrDefault() is RollPrerequisite _maxRoll)
                {
                    // compute total check value
                    var _checkVal = (double)(_maxRoll.RollValue + _charisma);
                    _suBurst.Context.Add(new ValueTarget<int>(@"Roll.CheckValue", Convert.ToInt32(_checkVal)));

                    // compute offset
                    var _maxOffSet = Convert.ToInt32(Math.Floor((_checkVal - 10d) / 3d));
                    if (_maxOffSet < -4)
                        _maxOffSet = -4;
                    if (_maxOffSet > 4)
                        _maxOffSet = 4;

                    // compute and store final max
                    var _finalMax = _infLevel + _maxOffSet;
                    var _dMax = new ValueTarget<int>(_maxRoll.BindKey, _finalMax);
                    _suBurst.Context.Add(_dMax);
                }

                // track capacity so we can change it as we consume it...
                if (_suBurst.Activation.AllPrerequisites<RollPrerequisite>(@"Roll.TotalPowerDice").FirstOrDefault() is RollPrerequisite _power)
                {
                    var _capacity = _power.RollValue + _infLevel + _charisma;
                    var _dPower = new ValueTarget<double>(_power.BindKey, Convert.ToDouble(_capacity));
                    _suBurst.Context.Add(_dPower);
                }
            }
            return;
        }
        #endregion

        #region public IEnumerable<CoreStep> Capture(BurstCapture burst, Locator locator)
        public IEnumerable<CoreStep> Capture(BurstCapture burst, Locator locator)
        {
            if (burst is PowerBurstCapture<SuperNaturalPowerActionSource> _suBurst)
            {

                // setup interaction for qualified checks
                var _creature = _suBurst.Activation.Actor as Creature;
                var _driveData = new DriveCreatureData(_creature, this);
                var _driveSet = new Interaction(_creature, _suBurst.Source, null, _driveData);

                // qualified actor level
                var _infLevel = _suBurst.PowerActionSource.PowerClass.ClassPowerLevel.QualifiedValue(_driveSet);

                // get remaining capacity
                var _power = _suBurst.Context.FirstOrDefault(_t => _t.Key.Equals(@"Roll.TotalPowerDice")) as ValueTarget<double>;
                var _remaining = _power.Value;
                if (_remaining > 0)
                {
                    // get check roll
                    var _checkRoll = _suBurst.Context.FirstOrDefault(_t => _t.Key.Equals(@"Roll.CheckValue")) as ValueTarget<int>;

                    // get max
                    if ((_suBurst.Context.FirstOrDefault(_t => _t.Key.Equals(@"Roll.MaxPowerDie")) is ValueTarget<int> _max) && (_max.Value > 0))
                    {
                        // get captured locators
                        var _delivery = _suBurst.Activation;
                        foreach (var _target in from _c in locator.AllConnected().OfType<Creature>()
                                                where !_c.Conditions.Contains(Condition.Dead)
                                                    && CreatureFilter.DoesMatch(_c)
                                                let _pl = _c.AdvancementLog.PowerLevel.QualifiedValue(_driveSet)
                                                where (_pl <= _max.Value)
                                                    && (_pl <= _remaining)
                                                select new { Target = new AimTarget(@"Target", _c), PowerLevel = _pl })
                        {
                            // create step (if possible and deliver)
                            var _step = CreateStep(_suBurst, _infLevel, _target.PowerLevel, _target.Target);

                            // if step was created and successful delivered, reduce remaining
                            if (_step is PowerApplyStep<SuperNaturalPowerActionSource>)
                            {
                                var _applyStep = _step as PowerApplyStep<SuperNaturalPowerActionSource>;
                                var _feedback = _applyStep.DeliveryInteraction.Feedback.OfType<PowerActionTransitFeedback<SuperNaturalPowerActionSource>>().FirstOrDefault();
                                if ((_feedback != null) && _feedback.Success)
                                {
                                    // if the effect was successfully delivered, decrease remaining capacity
                                    _remaining -= _target.PowerLevel;
                                }
                            }
                        }

                        // update the context (power dice remaining)
                        if (_remaining != _power.Value)
                        {
                            _power.Value = _remaining;
                        }
                    }
                }
            }
            yield break;
        }
        #endregion

        #region public IEnumerable<Locator> ProcessOrder(BurstCapture burst, IEnumerable<Locator> selection)
        public IEnumerable<Locator> ProcessOrder(BurstCapture burst, IEnumerable<Locator> selection)
        {
            if (burst is PowerBurstCapture<SuperNaturalPowerActionSource> _burst)
                return BurstCapture.OrderClosest(selection, burst.Origin.GetPoint3D());
            return selection;
        }
        #endregion

        #endregion

        /// <summary>Creates a CoreStep for the Creature</summary>
        public abstract CoreStep CreateStep(PowerBurstCapture<SuperNaturalPowerActionSource> burst, int classPowerLevel,
            int critterPowerLevel, AimTarget target);

        #region protected static void AddCheckValue(CoreStep target, PowerBurstCapture<SuperNaturalPowerSource> source)
        /// <summary>Adds checkValue to effects or powerTransit interaction data depending on power use results</summary>
        protected static void AddCheckValue(CoreStep target, PowerBurstCapture<SuperNaturalPowerActionSource> source)
        {
            if (target is PowerApplyStep<SuperNaturalPowerActionSource>)
            {
                var _step = target as PowerApplyStep<SuperNaturalPowerActionSource>;
                var _checkRoll = source.Context.FirstOrDefault(_t => _t.Key.Equals(@"Roll.CheckValue")) as ValueTarget<int>;

                // durable effect or direct power transit?
                // ... determines where additional targets are parked
                if (_step.DeliveryInteraction.InteractData is MagicPowerEffectTransit<SuperNaturalPowerActionSource> _transit)
                {
                    // add check values to magic effects (for Activation)
                    foreach (var _durable in _transit.MagicPowerEffects.OfType<DurableMagicEffect>())
                        _durable.AllTargets.Add(_checkRoll);
                }
                else
                {
                    if (_step.DeliveryInteraction.InteractData is PowerActionTransit<SuperNaturalPowerActionSource> _powerTransit)
                    {
                        // add check values to power transit (for ApplyPower)
                        _powerTransit.AllTargets.Add(_checkRoll);
                    }
                }
            }
        }
        #endregion
    }
}
