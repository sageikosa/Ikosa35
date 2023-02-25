using System;
using Uzi.Core;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class NotHealing : ActorStateBase, ITrackTime, IRecoveryAdjunct
    {
        public NotHealing(object source)
            : base(source)
        {
            _NextCheck = double.MaxValue;
            _Disabled = new Disabled(this);
        }

        private double _NextCheck;
        private Disabled _Disabled;

        #region Activate
        protected override void OnActivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                // add condition and disabled
                _critter.Conditions.Add(new Condition(Condition.NotHealing, this));
                _critter.AddAdjunct(_Disabled);

                // get next time
                var _time = CurrentTime;
                if (_time < (double.MaxValue - Day.UnitFactor))
                    _NextCheck = _time + Day.UnitFactor;
                else
                    _NextCheck = _time;

                // notify
                NotifyStateChange();
            }
            base.OnActivate(source);
        }
        #endregion

        #region DeActivate
        protected override void OnDeactivate(object source)
        {
            base.OnDeactivate(source);
            if (Anchor is Creature _critter)
            {
                // remove disabled
                _Disabled.Eject();

                // remove condition
                _critter.Conditions.Remove(_critter.Conditions[Condition.NotHealing, this]);
            }
        }
        #endregion

        public bool IsBarelyRecovering { get { return true; } }

        public override object Clone()
        {
            return new NotHealing(Source);
        }

        #region ITrackTime Members

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((timeVal >= _NextCheck) && (direction == TimeValTransition.Entering))
            {
                if (_NextCheck < (double.MaxValue - Day.UnitFactor))
                    _NextCheck += Day.UnitFactor;

                // setup roll split
                var _critter = Anchor as Creature;
                var _rollStep = new RollSplitStep((CoreProcess)null, @"Creature.NotHealing",
                    string.Format(@"10% start healing, or lose 1 HP ({0})", _critter != null ? _critter.Name : Anchor.ID.ToString()),
                    new DieRoller(100));
                var _process = new CoreProcess(_rollStep, @"Recovery Chance");
                _rollStep.PossibleSteps.Add(10, new ActorStateChangeStep(_process, this, new Disabled(Source), _critter));
                _rollStep.PossibleSteps.Add(100, new LoseHealthPointStep(_process, _critter));
                StartProcess(_process);
            }
        }

        public double Resolution
        {
            get { return Day.UnitFactor; }
        }

        #endregion
    }
}
