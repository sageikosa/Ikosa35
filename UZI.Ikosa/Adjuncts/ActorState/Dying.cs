using System;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Dying : ActorStateBase, ITrackTime
    {
        public Dying(object source)
            : base(source)
        {
            _Unconscious = new UnconsciousEffect(this, double.MaxValue, Round.UnitFactor);
        }

        #region private data
        private double _NextCheck;
        private UnconsciousEffect _Unconscious;
        #endregion

        #region Activate
        protected override void OnActivate(object source)
        {
            var _critter = Anchor as Creature;
            if (_critter != null)
            {
                // add condition and unconsciousness
                _critter.Conditions.Add(new Condition(Condition.Dying, this));
                _critter.AddAdjunct(_Unconscious);

                // get next time
                var _time = CurrentTime;
                if (_time < (double.MaxValue - Round.UnitFactor))
                    _NextCheck = _time + Round.UnitFactor;
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
            var _critter = Anchor as Creature;
            if (_critter != null)
            {
                // remove unconsciousness
                _Unconscious.Eject();

                // remove condition
                _critter.Conditions.Remove(_critter.Conditions[Condition.Dying, this]);
            }
        }
        #endregion

        public override object Clone()
        {
            return new Dying(Source);
        }

        #region ITrackTime Members

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((timeVal >= _NextCheck) && (direction == TimeValTransition.Entering))
            {
                if (_NextCheck < (double.MaxValue - Round.UnitFactor))
                    _NextCheck += Round.UnitFactor;

                // setup roll split
                var _critter = Anchor as Creature;
                var _rollStep = new RollSplitStep((CoreProcess)null, @"Creature.Dying",
                    string.Format(@"10% stabilization ({0})", _critter != null ? _critter.Name : Anchor.ID.ToString()),
                    new DieRoller(100));
                var _process = new CoreProcess(_rollStep, @"Stabilization Chance");
                _rollStep.PossibleSteps.Add(10, new ActorStateChangeStep(_process, this, new StableNatural(this.Source), _critter));
                _rollStep.PossibleSteps.Add(100, new LoseHealthPointStep(_process, _critter));
                StartProcess(_process);
            }
        }

        public double Resolution
        {
            get { return Round.UnitFactor; }
        }

        #endregion
    }
}
