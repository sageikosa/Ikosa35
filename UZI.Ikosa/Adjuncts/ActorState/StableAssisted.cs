using System;
using Uzi.Core;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class StableAssisted : ActorStateBase, ITrackTime, IRecoveryAdjunct
    {
        public StableAssisted(object source)
            : base(source)
        {
            _Unconscious = new UnconsciousEffect(this, double.MaxValue, Hour.UnitFactor);
        }

        #region private data
        private double _NextCheck;
        private UnconsciousEffect _Unconscious;
        #endregion

        #region Activate
        protected override void OnActivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                // add condition and unconsciousness
                _critter.Conditions.Add(new Condition(Condition.Stable, this));
                _critter.AddAdjunct(_Unconscious);

                // get next time
                var _time = CurrentTime;
                if (_time < (double.MaxValue - Hour.UnitFactor))
                    _NextCheck = _time + Hour.UnitFactor;
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
                // remove unconsciousness
                _Unconscious.Eject();

                // remove condition
                _critter.Conditions.Remove(_critter.Conditions[Condition.Stable, this]);
            }
        }
        #endregion

        public bool IsBarelyRecovering { get { return false; } }

        public override object Clone()
        {
            return new StableAssisted(Source);
        }

        #region ITrackTime Members

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((timeVal >= _NextCheck) && (direction == TimeValTransition.Entering))
            {
                if (_NextCheck < (double.MaxValue - Hour.UnitFactor))
                    _NextCheck += Hour.UnitFactor;

                // setup roll split
                var _critter = Anchor as Creature;
                var _rollStep = new RollSplitStep((CoreProcess)null, @"Creature.Stable",
                    string.Format(@"10% recover to disabled ({0})", _critter != null ? _critter.Name : Anchor.ID.ToString()),
                    new DieRoller(100));
                var _process = new CoreProcess(_rollStep, @"Recovery Chance");
                _rollStep.PossibleSteps.Add(10, new ActorStateChangeStep(_process, this, new Disabled(Source), _critter));
                _rollStep.PossibleSteps.Add(100, null);
                StartProcess(_process);
            }
        }

        public double Resolution
        {
            get { return Hour.UnitFactor; }
        }

        #endregion
    }
}
