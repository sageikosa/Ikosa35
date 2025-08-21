using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class ParalyzedEffect : PredispositionBase, ITrackTime
    {
        public ParalyzedEffect(object source, double endTime, double resolution, SaveMode saveMode)
            : base(source)
        {
            _EndTime = endTime;
            _Resolution = resolution;
            _SaveMode = saveMode;
        }

        #region data
        private double _EndTime;
        private double _Resolution;
        private SaveMode _SaveMode;
        #endregion

        public double EndTime => _EndTime;
        public double Resolution => _Resolution;
        public SaveMode SaveMode => _SaveMode;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);

            var _critter = Anchor as Creature;
            _critter.Conditions.Add(new Condition(Condition.Paralyzed, this));
            _critter.Abilities.Strength.SetZeroHold(this, true);
            _critter.AddAdjunct(new Immobilized(this, false));
        }

        protected override void OnDeactivate(object source)
        {
            var _critter = Anchor as Creature;
            _critter.Conditions.Remove(_critter.Conditions[Condition.Paralyzed, this]);
            _critter.Abilities.Strength.SetZeroHold(this, false);
            _critter.Adjuncts.OfType<Immobilized>().FirstOrDefault(_i => _i.Source == this)?.Eject();
            base.OnDeactivate(source);
        }

        public override string Description
            => @"Paralyzed";

        public override object Clone()
            => new ParalyzedEffect(Source, EndTime, Resolution, SaveMode);

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((timeVal >= EndTime) && (direction == TimeValTransition.Leaving))
            {
                Eject();
            }
        }
    }

    public interface IParalysisProvider
    {
        object QualifierSource { get; }
        SaveType SaveType { get; }
        IVolatileValue Difficulty { get; }
        Roller TimeUnits { get; }
        TimeUnit UnitFactor { get; }
        bool WillAffect(IInteract target);
    }

    [Serializable]
    public class ParalyzeStep : PreReqListStepBase
    {
        #region ctor()
        public ParalyzeStep(CoreProcess process, IParalysisProvider provider, Creature target)
            : base(process)
        {
            _Provider = provider;
            _Target = target;

            var _saveType = provider.SaveType;
            var _source = provider.QualifierSource;
            if (_saveType >= SaveType.Fortitude)
            {
                var _qualifier = new Qualifier(null, _source, target);
                var _difficulty = provider?.Difficulty?.GetDeltaCalcInfo(_qualifier, @"Paralysis Difficulty");
                if (_difficulty != null)
                {
                    _PendingPreRequisites.Enqueue(new SavePrerequisite(_source, _qualifier, @"Save", $@"{_saveType} Save",
                        new SaveMode(_saveType, SaveEffect.Negates, _difficulty)));
                }
            }

            var _units = provider.TimeUnits;
            if (!((_units is ConstantRoller) || (_units == null)))
            {
                _PendingPreRequisites.Enqueue(new RollPrerequisite(_source, @"Duration", @"Duration", _units, true));
            }
        }
        #endregion

        #region data
        private IParalysisProvider _Provider;
        private Creature _Target;
        #endregion

        public IParalysisProvider ParalysisProvider => _Provider;
        public Creature Target => _Target;

        #region bool OnDoStep()
        protected override bool OnDoStep()
        {
            if (IsComplete)
            {
                return true;
            }

            var _save = GetPrerequisite<SavePrerequisite>();
            if (_save != null)
            {
                if (_save.IsReady)
                {
                    if (_save.Success)
                    {
                        // inform
                        EnqueueNotify(new CheckResultNotify(Target.ID, @"Save", true, new Info { Message = @"versus paralysis" }), Target.ID);
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }

            var _roll = GetPrerequisite<RollPrerequisite>();
            if (_roll != null)
            {
                if (_roll.IsReady)
                {
                    var _timeFactor = ParalysisProvider.UnitFactor.BaseUnitFactor;
                    var _span = _roll.RollValue * _timeFactor;
                    var _curr = Target?.GetCurrentTime() ?? 0;
                    Target.AddAdjunct(new ParalyzedEffect(typeof(ParalyzedEffect), _curr + _span, _timeFactor, _save?.SaveMode));
                }
                else
                {
                    return false;
                }
            }
            else
            {
                // check for constant roller...
                var _units = ParalysisProvider.TimeUnits;
                if (_units is ConstantRoller)
                {
                    var _timeFactor = ParalysisProvider.UnitFactor.BaseUnitFactor;
                    var _span = _units.RollValue(Guid.Empty, @"Paralysis", ParalysisProvider.UnitFactor.PluralName) * _timeFactor;
                    var _curr = Target?.GetCurrentTime() ?? 0;
                    Target.AddAdjunct(new ParalyzedEffect(typeof(ParalyzedEffect), _curr + _span, _timeFactor, _save?.SaveMode));
                }
                else
                {
                    Target.AddAdjunct(new ParalyzedEffect(typeof(ParalyzedEffect), double.MaxValue, Year.UnitFactor, _save?.SaveMode));
                }
            }
            return true;
        }
        #endregion
    }
}
