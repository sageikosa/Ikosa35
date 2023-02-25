using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    // TODO: checks and activities that rely on vision (such as reading and Spot checks) automatically fail
    [Serializable]
    public class BlindEffect : Adjunct, ITrackTime, IMonitorChange<Activation>
    {
        #region Construction
        public BlindEffect(object source, double endTime, double resolution)
            : base(source)
        {
            _Senses = new List<SensoryBase>();
            _EndTime = endTime;
            _TimeRes = resolution;
        }
        #endregion

        #region data
        // senses that need to be reactivated...
        private List<SensoryBase> _Senses;
        private double _EndTime;
        private double _TimeRes;
        #endregion

        protected override void OnActivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                // disabled senses
                _critter.Conditions.Add(new Condition(Condition.Blinded, this));
                foreach (var _sense in from _s in _critter.Senses.AllSenses
                                       where _s.UsesSight && _s.IsActive
                                       select _s)
                {
                    _sense.IsActive = false;
                    if (!_sense.IsActive)
                    {
                        _sense.AddChangeMonitor(this);
                        _Senses.Add(_sense);
                    }
                }

                _critter.Awarenesses.RecalculateAwareness(_critter);
                // TODO: add other penalties...
            }
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                // re-enable senses
                _critter.Conditions.Remove(_critter.Conditions[Condition.Blinded, this]);
                foreach (var _sense in _Senses)
                {
                    _sense.RemoveChangeMonitor(this);
                    _sense.IsActive = true;
                }
                _Senses.Clear();

                _critter.Awarenesses.RecalculateAwareness(_critter);

                // TODO: remove other penalties...
            }
            base.OnDeactivate(source);
        }

        #region ITrackTime Members
        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((timeVal >= _EndTime) && (direction == TimeValTransition.Entering))
            {
                Eject();
            }
        }

        public double EndTime => _EndTime;
        public double Resolution => _TimeRes;
        #endregion

        #region IMonitorChange<Activation> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<Activation> args)
        {
            if (args.NewValue.IsActive)
                args.DoAbort();
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<Activation> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<Activation> args)
        {
        }
        #endregion

        public override object Clone()
            => new BlindEffect(Source, EndTime, Resolution);
    }
}
